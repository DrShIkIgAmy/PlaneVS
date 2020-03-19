using System;
using System.Collections.Generic;
using Urho;
using Urho.Actions;
using Urho.Physics;
using Urho.Urho2D;

namespace Planer
{
    public class Obstecles:Component
    {
        public Vector3 trackSize { get; set; }
        public Vector3 Size { get; set; }
        public float Interval { get; set; }
        public float StartInterval { get; set; }
		public float Speed { get; set; }
		List<Obstecle> obstecles = new List<Obstecle>();
		public delegate void scoreReceived();
		public event scoreReceived scoreAchieved;
        public void start()
        {
			int obstCount = (int)Math.Floor(trackSize.Z / Interval);
			float curZ = StartInterval;
			for(int i = 0;i<obstCount;i++)
			{
				var obst = new Obstecle();
				Node.AddComponent(obst);
				obst.Speed = Speed;
				obst.trackSize = trackSize;
				obst.Size = Size;
				float curX = RandomHelper.NextRandom(-trackSize.X/2, trackSize.X/2);
				obst.LastPosition = new Vector3(curX, 50, -300);
				obst.Position = new Vector3(curX, 50, curZ += Interval);
				obstecles.Add(obst);
				obstecles.FindLast(x => true).scoreAchieved+=throwEvent;
			}
			foreach (var i in obstecles)
			{
				i.FirstPosition = new Vector3(trackSize.X, 50, curZ);
				i.Start();
			}
		}
		public void throwEvent()
		{
			scoreAchieved?.Invoke();
		}
		public void stop()
		{
			foreach(var i in obstecles)
			{
				i.Destroy();
			}
		}
	}

	public class Obstecle:Component
	{
		Node parentNode = null;
		Node obstNode = null;
		public Vector3 Size { get; set; }

		public delegate void Passed();
		public event Passed scoreAchieved;

		public Vector3 Position { get; set; }
		public Vector3 trackSize { get; set; }
		public float Speed { get; set; }
		public Vector3 LastPosition { get; set; }
		public Vector3 FirstPosition { get; set; }
		public void Create()
		{
			if(parentNode==null)
				parentNode = Node.CreateChild();
			obstNode = parentNode.CreateChild();
			var cach = Application.ResourceCache;
			obstNode = Node.CreateChild();
			obstNode.Scale = Size;
			obstNode.Position = Position;
			obstNode.Rotation = new Quaternion(0, 90, 0);
			var obstecle = obstNode.CreateComponent<StaticModel>();
			obstecle.Model = cach.GetModel(Assets.Models.WaterTower);
			obstecle.Material = cach.GetMaterial(Assets.Materials.WaterTower);

			var TailNode = obstNode.CreateChild();
			//TailNode.Translate(new Vector3(0, 0, Size.Z / 2));
			TailNode.Scale = new Vector3(100, 100, 100);
			var Tail = TailNode.CreateComponent<ParticleEmitter2D>();
			Tail.Effect = cach.GetParticleEffect2D(Assets.Particles.ObstTail);

			var body = obstNode.CreateComponent<RigidBody>();
			body.Mass = 1;
			body.Kinematic = true;
			body.CollisionLayer = (uint)4;
			var collision = obstNode.CreateComponent<CollisionShape>();
			var hitBoxSize = obstecle.BoundingBox.Size;
			hitBoxSize.Scale(0.5f, 0.5f, 0.5f);
			collision.SetBox(hitBoxSize, new Vector3(0, 0, 0), new Quaternion(0, 0, 0));
		}
		public void Start()
		{
			_startAsync();
		}
		private async void _startAsync()
		{
			while (true)
			{
				Create();
				float time = -(LastPosition.Z - Position.Z) / Speed;
				
				await obstNode.RunActionsAsync(new MoveTo(time, LastPosition));
				obstNode.SetWorldPosition(FirstPosition);
				
				var curX = RandomHelper.NextRandom(-trackSize.X/2, trackSize.X / 2);
				FirstPosition = new Vector3(curX, FirstPosition.Y, FirstPosition.Z);
				LastPosition = new Vector3(curX, LastPosition.Y, LastPosition.Z);
				Position = FirstPosition;
				obstNode.Remove();
				scoreAchieved?.Invoke();
			}
		}
		public void Destroy()
		{
			obstNode.RemoveAllActions();
			obstNode.RemoveAllComponents();
			obstNode.RemoveAllChildren();
			parentNode.RemoveAllChildren();
			parentNode.Remove();
		}
	}

	public class RandomHelper
	{
		static readonly Random Random = new Random();

		/// <summary>
		/// Return a random float between min and max, inclusive from both ends.
		/// </summary>
		public static float NextRandom(float min, float max) => (float)((Random.NextDouble() * (max - min)) + min);

		/// <summary>
		/// Return a random integer between min and max - 1.
		/// </summary>
		public static int NextRandom(int min, int max) => Random.Next(min, max);

		/// <summary>
		/// Return a random boolean
		/// </summary>
		public static bool NextBoolRandom() => Random.Next(0, 2) == 1;
	}
}
