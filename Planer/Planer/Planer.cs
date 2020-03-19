using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Physics;
using Urho.Urho2D;

namespace Planer
{
    class Planer : Component
    {
        Urho.Resources.ResourceCache cach;
        public Vector3 planeScale { get; set; }
        public Vector3 planePosition { get; set; }
        public delegate void PlanerCrashed();
        public event PlanerCrashed GameOver;
        Node planerNode = null;
        public void Init()
        {
            cach = Application.ResourceCache;
            planerNode = Node.CreateChild(StringNodesNames.ShipNode);
            planerNode.Scale = planeScale;
            planerNode.Position = planePosition;
            var plane = planerNode.CreateComponent<StaticModel>();
            plane.Model = cach.GetModel(Assets.Models.VerBot);
            plane.Material = cach.GetMaterial(Assets.Materials.VerBot);

            var bodyNode = planerNode.CreateChild(StringNodesNames.ShipBody);
            bodyNode.Scale = new Vector3(1, 1, 1);
            var body = bodyNode.CreateComponent<RigidBody>();
            body.Kinematic = true;
            body.Mass = 1;
            body.CollisionLayer = (uint)2;

            var hitBoxSize = plane.BoundingBox.Size;
            hitBoxSize.Scale(0.5f, 0.5f, 0.5f);
            
            var collisionShape = bodyNode.CreateComponent<CollisionShape>();
            collisionShape.SetBox(hitBoxSize, new Vector3(0, 0, 0), new Quaternion(0, 0, 0));
            collisionShape.Model = cach.GetModel(Assets.Models.Box);
            bodyNode.NodeCollisionStart += onCollided;

            var tailNode = planerNode.CreateChild();
            tailNode.SetScale(1f);
            var particles = tailNode.CreateComponent<ParticleEmitter2D>();
            particles.Effect = cach.GetParticleEffect2D(Assets.Particles.Tail);

        }
        public async Task Crash()
        {
            var ExplosionNode = planerNode.CreateChild();
            var Explosion = ExplosionNode.CreateComponent<ParticleEmitter2D>();
            Explosion.Effect = cach.GetParticleEffect2D(Assets.Particles.Crashed);
            await ExplosionNode.RunActionsAsync(new ScaleTo(0.2f, 10));
            await ExplosionNode.RunActionsAsync(new ScaleTo(0.2f, 0.0001f));
        }
        void onCollided(NodeCollisionStartEventArgs e)
        {
            GameOver?.Invoke();
        }
        public void changePosition(Vector3 position,float angle)
        {
            planerNode.Rotation = new Quaternion(0, 0, angle);
            planerNode.SetWorldPosition(position);
        }
    }
}
