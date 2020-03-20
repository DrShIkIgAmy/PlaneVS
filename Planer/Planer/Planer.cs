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
    class Ship : Component
    {
        private Urho.Resources.ResourceCache _cach;
        public Vector3 Scale { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 HitBoxScale { get; set; }

        public delegate void ShipCrashed();
        public event ShipCrashed Crashed;

        private Node _shipNode = null;
        private Node _shipBodyNode = null;
        private StaticModel _ship = null;

        public void Init()
        {
            _cach = Application.ResourceCache;
            CreateShipModel();
            CreateShipBody();
            CreateShipCollisionLayer();
            CreateShipTail();
            _shipBodyNode.NodeCollisionStart += onCollided;
        }

        private void CreateShipModel()
        {
            _shipNode = Node.CreateChild(StringNodesNames.ShipNode);
            _shipNode.Scale = Scale;
            _shipNode.Position = Position;
            _shipNode.Rotation = Rotation;
            _ship = _shipNode.CreateComponent<StaticModel>();
            _ship.Model = _cach.GetModel(Assets.Models.VerBot);
            _ship.Material = _cach.GetMaterial(Assets.Materials.VerBot);
        }
        private void CreateShipBody()
        {
            _shipBodyNode = _shipNode.CreateChild(StringNodesNames.ShipBody);
            _shipBodyNode.Scale = new Vector3(1, 1, 1);
            var body = _shipBodyNode.CreateComponent<RigidBody>();
            body.Kinematic = true;
            body.Mass = 1;
            body.CollisionLayer = (uint)2;
        }
        private void CreateShipCollisionLayer()
        {
            var hitBoxSize = _ship.BoundingBox.Size;
            hitBoxSize.Scale(HitBoxScale);
            var collisionShape = _shipBodyNode.CreateComponent<CollisionShape>();
            collisionShape.SetBox(hitBoxSize, new Vector3(0, 0, 0), new Quaternion(0, 0, 0));
            collisionShape.Model = _cach.GetModel(Assets.Models.Box);
        }
        private void CreateShipTail()
        {
            var tailNode = _shipNode.CreateChild();
            tailNode.SetScale(1f);
            var particles = tailNode.CreateComponent<ParticleEmitter2D>();
            particles.Effect = _cach.GetParticleEffect2D(Assets.Particles.Tail);
        }

        public async Task BlowUp()
        {
            var ExplosionNode = _shipNode.CreateChild();
            var Explosion = ExplosionNode.CreateComponent<ParticleEmitter2D>();
            Explosion.Effect = _cach.GetParticleEffect2D(Assets.Particles.Crashed);
            await ExplosionNode.RunActionsAsync(new ScaleTo(0.2f, 10));
            await ExplosionNode.RunActionsAsync(new ScaleTo(0.2f, 0.0001f));
        }
        void onCollided(NodeCollisionStartEventArgs e)
        {
            Crashed?.Invoke();
        }
        public void changePosition(Vector3 position,float angle)
        {
            _shipNode.Rotation = new Quaternion(Rotation.X, Rotation.Y, angle);
            _shipNode.SetWorldPosition(position);
        }
    }
}
