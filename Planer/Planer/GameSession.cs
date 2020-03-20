using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Physics;
using Xamarin.Essentials;

namespace Planer
{
	public class GameSession : Component
	{
		public delegate void GameOver();
		public event GameOver GameOvered;

		private Scene _scene = null;
		private Ship _ship = null;
		private Obstecles _obstecles = null;
		private Node _lightNode = null;
		private Node _trackNode = null;

		private ControllDataHendler _filter = null;
		private Text _scoreLabel = null;
		private bool _gameOver = false;
		private bool _finished = false;

		public int Scores { get; set; } = 0;

		public Vector3 WorldGravity { get; set; } = new Vector3(0, 0, 0);

		public Vector3 CameraPosition { get; set; } = new Vector3(0.0f, 400f, -300.0f);
		public Quaternion CameraRotation { get; set; } = new Quaternion(45, 0, 0);

		public Vector3 LightPosition { get; set; } = new Vector3(0.0f, 200f, 50);
		public Quaternion LightRotation { get; set; } = new Quaternion(0, 0, 0);

		public Vector3 TrackSize { get; set; } = new Vector3(200, 1, 1000);
		public Vector3 TrackPosition { get; set; } = new Vector3(0, 0, 200);

		public Vector3 ShipSize { get; set; } = new Vector3(10, 10, 10);
		public Vector3 ShipPosition { get; set; } = new Vector3(0, 300, -50);
		public Quaternion ShipRotation { get; set; } = new Quaternion(0, 0, 0);

		public Vector3 ObsteclesSize { get; set; } = new Vector3(0.3f, 0.3f, 0.3f);
		public float ObsteclesInterval { get; set; } = 400;
		public float ObsteclesSpeed { get; set; } = 200;

		public void AddScene(ref Scene scene)
		{
			_scene = scene;
		}

		public async Task StopGame()
		{
			DetachAcc();
			_gameOver = true;
			await _ship.BlowUp();
			_obstecles.stop();
			_scene?.RemoveAllComponents();
			_scene?.RemoveAllChildren();
			Application.UI.Root.RemoveAllChildren();
			GameOvered?.Invoke();
		}

		public void StartGame()
		{
			_gameOver = false;
			Scores = 0;
			_filter?.reset();
			CreateScene();
			RunShip();
			RunObstecles();
			AttachAcc();
		}

		void CreateScene()
		{
			if (_scene == null) 
				_scene = new Scene();

			_scene.CreateComponent<Octree>();

			var physics = _scene.CreateComponent<PhysicsWorld>();
			physics.SetGravity(WorldGravity);

			var cameraNode = _scene.CreateChild();
			cameraNode.Position = CameraPosition;
			cameraNode.Rotate(CameraRotation);
			cameraNode.CreateComponent<Camera>();
			var Viewport = new Viewport(Context, _scene, cameraNode.GetComponent<Camera>(), null);

			if (Application.Platform != Platforms.Android && Application.Platform != Platforms.iOS)
			{
				RenderPath effectRenderPath = Viewport.RenderPath.Clone();
				var fxaaRp = Application.ResourceCache.GetXmlFile(Assets.PostProcess.FXAA3);
				effectRenderPath.Append(fxaaRp);
				Viewport.RenderPath = effectRenderPath;
			}

			Application.Renderer.SetViewport(0, Viewport);

			var zoneNode = _scene.CreateChild();
			var zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
			zone.AmbientColor = new Color(1f, 1f, 1f);


			_trackNode = _scene.CreateChild();
			_trackNode.Scale = TrackSize;
			_trackNode.Position = TrackPosition;
			var track = _trackNode.CreateComponent<AnimatedModel>();
			track.Model = Application.ResourceCache.GetModel(Assets.Models.Plane);
			track.Material = Application.ResourceCache.GetMaterial(Assets.Materials.Grass);

			_lightNode = _scene.CreateChild();
			_lightNode.Position = LightPosition;
			_lightNode.Rotation = LightRotation;
			_lightNode.AddComponent(new Light { Range = 1200, Brightness = 0.8f });

			_scoreLabel = new Text();
			_scoreLabel.HorizontalAlignment = HorizontalAlignment.Right;
			_scoreLabel.VerticalAlignment = VerticalAlignment.Top;
			_scoreLabel.SetFont(Application.ResourceCache.GetFont(Assets.Fonts.Font), Application.Graphics.Width / 20);
			_scoreLabel.Value = 0.ToString();
			Application.UI.Root.AddChild(_scoreLabel);
		}

		void RunShip()
		{
			_ship = new Ship();
			_ship.Position = ShipPosition;
			_ship.Scale = ShipSize;
			_ship.Rotation = ShipRotation;
			_scene.AddComponent(_ship);
			_ship.Init();
			_ship.Crashed += onCrash;
		}

		void RunObstecles()
		{
			_obstecles = new Obstecles();
			_obstecles.scoreAchieved += incrementScore;
			_obstecles.TrackSize = TrackSize;
			_obstecles.Size = ObsteclesSize;
			_obstecles.Interval = ObsteclesInterval;
			_obstecles.Speed = ObsteclesSpeed;
			_scene.AddComponent(_obstecles);
			_obstecles.Init();
			_obstecles.start();

		}

		void AttachAcc()
		{
			if (Accelerometer.IsMonitoring)
				return;
			Accelerometer.ReadingChanged += controll;
			Accelerometer.Start(SensorSpeed.Game);
		}

		void DetachAcc()
		{
			if (!Accelerometer.IsMonitoring)
				return;
			Accelerometer.ReadingChanged -= controll;
			Accelerometer.Stop();
		}
		
		void incrementScore()
		{
			++Scores;
			_scoreLabel.Value = Scores.ToString();
		}

		public void controll(object sender, AccelerometerChangedEventArgs e)
		{
			if (_ship == null)
				return;
			if (_filter == null)
				_filter = new ControllDataHendler();
			_filter.procceseInpAcc(e.Reading.Acceleration.X);
			_ship.changePosition(new Vector3(-_filter.Velocity, 50, -50), _filter.AngleDelta);
		}

		async void onCrash()
		{
			await StopGame();
		}

	}

	public class ControllDataHendler
	{
		public float Velocity { get; private set; } = 0;
        public float AngleDelta { get; private set; }
		public int XLimit { get; set; } = 120;

		private float _velocityPrev;
		private float _pppAngle;
		private float _ppAnle;
		private float _pAngle;
		private float _alpha = 0.4f, _betta = 0.3f, _gamma = 0.2f, _tetta = 0.1f;
		private float _angle = 0;

		public void SetExpFilter(float Alpha,float Betta,float Gamma,float Tetta)
		{
			_alpha = Alpha;
			_betta = Betta;
			_gamma = Gamma;
			_tetta = Tetta;
		}

		public ControllDataHendler()
		{
			_velocityPrev = 0;
			_pppAngle = 0;
			_ppAnle = 0;
			_pAngle = 0;
		}

		public void procceseInpAcc(float inp)
		{
			var Angle = inp * _alpha + _pAngle * _betta + _ppAnle * _gamma + _pppAngle * _tetta;
			_pppAngle = _ppAnle;
			_ppAnle = _pAngle;
			_pAngle = Angle;
			Velocity += (Angle * 20);
			Velocity = Math.Abs(Velocity) > XLimit ? Math.Sign(Velocity) * XLimit : Velocity;
			_angle += Math.Abs(_angle) > 90 ? Math.Sign(_angle) * 90 - _angle : (Velocity - _velocityPrev) / 3f;
			AngleDelta = Math.Abs(_angle) >= 90 ? 0 : (Velocity - _velocityPrev) / 0.3f;
			_velocityPrev = Velocity;
		}

		public void reset()
		{
			Velocity = 0;
			AngleDelta = 0;

			_velocityPrev = 0;
			_pppAngle=0;
			_ppAnle=0;
			_pAngle=0;
			_angle = 0;
		}	
	}


}
