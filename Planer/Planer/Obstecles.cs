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
        public Vector3 TrackSize { get; set; }


        public Vector3 Size { get; set; }
        public float Interval { get; set; }
        public float Offset { get; set; }
		public float Speed { get; set; }
		public float Distance { get; set; }
		public float Yposition { get; set; } = 50;
		public float OverZdistance { get; set; } = -300;
		public int ObstecleCount { get; private set; }
		public float StartZ { get; private set; }

		List<Obstecle> _obsteclesList = new List<Obstecle>();

		public delegate void scoreReceived();
		public event scoreReceived scoreAchieved;

		public void Init()
		{
			ObstecleCount = (int)Math.Floor(TrackSize.Z / Interval);
			StartZ = Offset;
			for(int i=0;i<ObstecleCount;i++)
			{
				float curX = RandomHelper.NextRandom(-TrackSize.X / 2, TrackSize.X / 2);

				var obst = new Obstecle();
				Node.AddComponent(obst);

				obst.Speed = Speed;
				obst.Spread = new Vector2(-TrackSize.X / 2, TrackSize.X / 2);
				obst.Size = Size;
				obst.FrontTrackPosition = new Vector2(curX, OverZdistance);
				obst.InitPosition = new Vector3(curX, Yposition, StartZ += Interval);
				_obsteclesList.Add(obst);
				_obsteclesList.FindLast(x => true).scoreAchieved += throwEvent;
			}
			foreach (var i in _obsteclesList)
			{
				i.TailTrackPosition = new Vector2(TrackSize.X, StartZ);
			}
		}
        public void start()
        {
			foreach (var i in _obsteclesList)
			{
				i.Start();
			}
		}
		public void throwEvent()
		{
			scoreAchieved?.Invoke();
		}
		public void stop()
		{
			foreach(var i in _obsteclesList)
			{
				i.Destroy();
			}
		}
	}

	public class Obstecle:Component
	{
		Node _parentNode = null;
		Node _obstNode = null;
		public Vector3 Size { get; set; }
		public Vector3 InitPosition { get; set; }
		public Vector2 Spread { get; set; }
		public float Speed { get; set; }
		public Vector2 FrontTrackPosition { get; set; }
		public Vector2 TailTrackPosition { get; set; }
		public bool IsOnTrack { get; } = true;
		public delegate void Passed();

		public event Passed scoreAchieved;

		public void Create()
		{
			if(_parentNode==null)
				_parentNode = Node.CreateChild();
			_obstNode = _parentNode.CreateChild();
			var cach = Application.ResourceCache;
			_obstNode = Node.CreateChild();
			_obstNode.Scale = Size;
			_obstNode.Position = InitPosition;
			_obstNode.Rotation = new Quaternion(0, 90, 0);
			var obstecle = _obstNode.CreateComponent<StaticModel>();
			obstecle.Model = cach.GetModel(Assets.Models.WaterTower);
			obstecle.Material = cach.GetMaterial(Assets.Materials.WaterTower);

			var TailNode = _obstNode.CreateChild();
			TailNode.Scale = new Vector3(100, 100, 100);
			var Tail = TailNode.CreateComponent<ParticleEmitter2D>();
			Tail.Effect = cach.GetParticleEffect2D(Assets.Particles.ObstTail);

			var body = _obstNode.CreateComponent<RigidBody>();
			body.Mass = 1;
			body.Kinematic = true;
			body.CollisionLayer = (uint)4;
			var collision = _obstNode.CreateComponent<CollisionShape>();
			var hitBoxSize = obstecle.BoundingBox.Size;
			hitBoxSize.Scale(0.9f, 0.9f, 0.9f);
			collision.SetBox(hitBoxSize, new Vector3(0, 0, 0), new Quaternion(0, 0, 0));
		}

		public void Start()
		{
			_startAsync();
		}
		private async void _startAsync()
		{
			while (IsOnTrack)
			{
				Create();
				float time = -(FrontTrackPosition.Y - InitPosition.Z) / Speed;
				
				await _obstNode.RunActionsAsync(new MoveTo(time, new Vector3(FrontTrackPosition.X,InitPosition.Y,FrontTrackPosition.Y)));
				_obstNode.SetWorldPosition(new Vector3(TailTrackPosition.X,InitPosition.Y,TailTrackPosition.Y));
				
				var curX = RandomHelper.NextRandom(-Spread.X/2, Spread.X / 2);

				TailTrackPosition = new Vector2(curX, TailTrackPosition.Y);
				FrontTrackPosition = new Vector2(curX, FrontTrackPosition.Y);
				InitPosition = new Vector3(TailTrackPosition.X, InitPosition.Y, TailTrackPosition.Y);
				_obstNode.Remove();
				scoreAchieved?.Invoke();
			}
		}

		public void Destroy()
		{
			_obstNode.RemoveAllActions();
			_obstNode.RemoveAllComponents();
			_obstNode.RemoveAllChildren();
			_parentNode.RemoveAllChildren();
			_parentNode.Remove();
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
