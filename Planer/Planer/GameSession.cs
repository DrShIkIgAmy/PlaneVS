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
    public class GameSession:Application
    {
        public GameSession(ApplicationOptions opts) : base(opts) { }

		Planer plane;
		Text score;
		Obstecles obstecles;
		Scene scene;
		ControllDataHendler filter;
		Vector3 trackSize = new Vector3(200, 1, 1000);
		int Score = 0;
		bool gameOver = false;
		bool finished = false;


		protected override void Start()
        {
            base.Start();
			CreateMenu();
        }
		async Task StopGame()
		{
			Accelerometer.ReadingChanged -= controll;
			Accelerometer.Stop();
			if (scene!=null)
			{
				obstecles.stop();
				await plane.Crash();
				scene.Clear();
				UI.Root.RemoveAllChildren();
				gameOver = true;
			}
			CreateMenu();
		}

		void StartGame()
		{
			gameOver = false;
			Score = 0;
			CreateScene();
			Accelerometer.ReadingChanged += controll;
			Accelerometer.Start(SensorSpeed.Game);
		}

		void CreateScene()
		{
			if (scene == null) 
				scene = new Scene();

			scene.CreateComponent<Octree>();

			var physics = scene.CreateComponent<PhysicsWorld>();
			physics.SetGravity(new Vector3(0, 0, 0));

			var cameraNode = scene.CreateChild();
			cameraNode.Position = (new Vector3(0.0f, 400f, -300.0f));
			cameraNode.Rotate(new Quaternion(45, 0, 0));
			cameraNode.CreateComponent<Camera>();
			var Viewport = new Viewport(Context, scene, cameraNode.GetComponent<Camera>(), null);

			if (Platform != Platforms.Android && Platform != Platforms.iOS)
			{
				RenderPath effectRenderPath = Viewport.RenderPath.Clone();
				var fxaaRp = ResourceCache.GetXmlFile(Assets.PostProcess.FXAA3);
				effectRenderPath.Append(fxaaRp);
				Viewport.RenderPath = effectRenderPath;
			}

			Renderer.SetViewport(0, Viewport);

			var zoneNode = scene.CreateChild();
			var zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
			zone.AmbientColor = new Color(1f, 1f, 1f);


			var landNode = scene.CreateChild();
			landNode.Scale = trackSize;
			landNode.Position = new Vector3(0, 0, 200);
			var land = landNode.CreateComponent<AnimatedModel>();
			land.Model = ResourceCache.GetModel(Assets.Models.Plane);
			land.Material = ResourceCache.GetMaterial(Assets.Materials.Grass);

			var lightNode = scene.CreateChild();
			lightNode.Position = new Vector3(0.0f, 200f, 50);
			lightNode.AddComponent(new Light { Range = 1200, Brightness = 0.8f });

			plane = new Planer();
			plane.planePosition = new Vector3(0, 300, -50);
			plane.planeScale = new Vector3(10f,10f,10f);
			scene.AddComponent(plane);
			plane.Init();
			plane.GameOver += onCrash;

			score = new Text();
			score.HorizontalAlignment = HorizontalAlignment.Right;
			score.VerticalAlignment = VerticalAlignment.Top;
			score.SetFont(ResourceCache.GetFont(Assets.Fonts.Font), Graphics.Width / 20);
			score.Value = 0.ToString();
			UI.Root.AddChild(score);

			obstecles = new Obstecles();
			obstecles.scoreAchieved += incrementScore;
			obstecles.trackSize = trackSize;
			obstecles.Size = new Vector3(0.3f, 0.3f, 0.3f);
			obstecles.Interval = 400;
			obstecles.Speed = 200;
			scene.AddComponent(obstecles);
			obstecles.start();
		}

		void CreateMenu()
		{
			if (scene == null)
				scene = new Scene();
			scene.CreateComponent<Octree>();

			var physics = scene.CreateComponent<PhysicsWorld>();
			physics.SetGravity(new Vector3(0, 0, 0));
			var cameraNode = scene.CreateChild();
			cameraNode.Position = (new Vector3(0.0f, 0f, -500.0f));
			cameraNode.Rotate(new Quaternion(10, 0, 0));
			cameraNode.CreateComponent<Camera>();
			var Viewport = new Viewport(Context, scene, cameraNode.GetComponent<Camera>(), null);

			if (Platform != Platforms.Android && Platform != Platforms.iOS)
			{
				RenderPath effectRenderPath = Viewport.RenderPath.Clone();
				var fxaaRp = ResourceCache.GetXmlFile(Assets.PostProcess.FXAA3);
				effectRenderPath.Append(fxaaRp);
				Viewport.RenderPath = effectRenderPath;
			}
			Renderer.SetViewport(0, Viewport);

			var zoneNode = scene.CreateChild();
			var zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-300.0f, 300.0f));
			zone.AmbientColor = new Color(1f, 1f, 1f);

			var plannerNode = scene.CreateChild();
			plannerNode.Position = new Vector3(0, -50, 0);
			plannerNode.Scale = new Vector3(10, 10, 10);
			var planner = plannerNode.CreateComponent<StaticModel>();
			planner.Model = ResourceCache.GetModel(Assets.Models.VerBot);
			planner.Material = ResourceCache.GetMaterial(Assets.Materials.VerBot);
			planner.SetMaterial(ResourceCache.GetMaterial(Assets.Materials.VerBot));
			var movement = new RepeatForever(new RotateBy(1, 0, 5, 0));
			plannerNode.RunActionsAsync(movement);
			// Lights:
			var lightNode = scene.CreateChild();
			lightNode.Position = new Vector3(0.0f, 0f, -5.0f);
			var light = lightNode.CreateComponent<Light>();
			light.Range = 1200;
			light.Brightness = 2;
			//lightNode.AddComponent(new Light { Range = 1200, Brightness = 0.8f });

			if(gameOver)
			{
				Text ResultText = new Text();
				ResultText.HorizontalAlignment = HorizontalAlignment.Center;
				ResultText.VerticalAlignment = VerticalAlignment.Center;
				ResultText.Value = "Game over with " + Score.ToString() + " score";
				ResultText.SetFont(ResourceCache.GetFont(Assets.Fonts.Font), Graphics.Width / 20);
				UI.Root.AddChild(ResultText);
			}

			Text WelcomeText = new Text();
			WelcomeText.HorizontalAlignment = HorizontalAlignment.Center;
			WelcomeText.VerticalAlignment = VerticalAlignment.Center;
			WelcomeText.Position = new IntVector2(WelcomeText.Position.X, 150);
			WelcomeText.Value = "Tap to play";
			WelcomeText.SetFont(ResourceCache.GetFont(Assets.Fonts.Font), Graphics.Width / 20);
			UI.Root.AddChild(WelcomeText);
			finished = false;

		}

		async void onCrash()
		{
			await StopGame();
		}
		
		void incrementScore()
		{
			++Score;
			score.Value = Score.ToString();
		}

		protected override async void OnUpdate(float timeStep)
		{
			if (finished)
				return;
			var input = Input;
			if (input.GetMouseButtonDown(MouseButton.Left) || input.NumTouches > 0)
			{
				finished = true;
				scene?.Clear();
				UI.Root.RemoveAllChildren();
				StartGame();
			}
		}

		public void controll(object sender, AccelerometerChangedEventArgs e)
		{
			if (plane == null)
				return;
			if (filter == null)
				filter = new ControllDataHendler();
			filter.procceseInpAcc(e.Reading.Acceleration.X);
			plane.changePosition(new Vector3(-filter.Velocity, 50, -50), filter.delta);
		}

	}

	public class ControllDataHendler
	{
		public float Velocity { get; set; }
		public float delta { get; set; }

		float VelocityPrev = 0;
		float pppAngle;
		float ppAnle;
		float pAngle;
		float alpha = 0.4f, betta = 0.3f, gamma = 0.2f, tetta = 0.1f;
		int xLimit = 100;
		float angle = 0;
		public void procceseInpAcc(float inp)
		{
			var Angle = inp * alpha + pAngle * betta + ppAnle * gamma + pppAngle * tetta;
			pppAngle = ppAnle;
			ppAnle = pAngle;
			pAngle = Angle;
			Velocity += (Angle * 20);
			Velocity = Math.Abs(Velocity) > xLimit ? Math.Sign(Velocity) * xLimit : Velocity;
			angle += Math.Abs(angle) > 90 ? Math.Sign(angle) * 90 - angle : (Velocity - VelocityPrev) / 3f;
			delta = Math.Abs(angle) >= 90 ? 0 : (Velocity - VelocityPrev) / 0.3f;
			VelocityPrev = Velocity;
		}
		public void reset()
		{
			Velocity = 0;
			delta = 0;

			VelocityPrev = 0;
			pppAngle=0;
			ppAnle=0;
			pAngle=0;
			angle = 0;
	}
	}


}
