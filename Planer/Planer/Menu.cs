using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Physics;
using Urho.Resources;
using Xamarin.Essentials;

namespace Planer
{
    class Menu:Component
    {
        private Urho.Resources.ResourceCache _cach;
		private Scene _scene = null;

		public string WelcomeString { get; set; }
		public string ResultString { get; set; } = "";
		public void AddScene(ref Scene scene)
		{
			_scene = scene;
		}
        public void CreateMenu()
        {
			if (_scene == null)
				_scene = new Scene();
			_scene.CreateComponent<Octree>();

			var physics = _scene.CreateComponent<PhysicsWorld>();
			physics.SetGravity(new Vector3(0, 0, 0));
			var cameraNode = _scene.CreateChild();
			cameraNode.Position = (new Vector3(0.0f, 0f, -500.0f));
			cameraNode.Rotate(new Quaternion(10, 0, 0));
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
			zone.SetBoundingBox(new BoundingBox(-300.0f, 300.0f));
			zone.AmbientColor = new Color(1f, 1f, 1f);

			var plannerNode = _scene.CreateChild();
			plannerNode.Position = new Vector3(0, -50, 0);
			plannerNode.Scale = new Vector3(10, 10, 10);
			var planner = plannerNode.CreateComponent<StaticModel>();
			planner.Model = Application.ResourceCache.GetModel(Assets.Models.VerBot);
			planner.Material = Application.ResourceCache.GetMaterial(Assets.Materials.VerBot);
			planner.SetMaterial(Application.ResourceCache.GetMaterial(Assets.Materials.VerBot));
			var movement = new RepeatForever(new RotateBy(1, 0, 5, 0));
			plannerNode.RunActionsAsync(movement);
			// Lights:
			var _lightNode = _scene.CreateChild();
			_lightNode.Position = new Vector3(0.0f, 0f, -5.0f);
			var light = _lightNode.CreateComponent<Light>();
			light.Range = 1200;
			light.Brightness = 2;

			if (ResultString!="")
			{
				Text ResultText = new Text();
				ResultText.HorizontalAlignment = HorizontalAlignment.Center;
				ResultText.VerticalAlignment = VerticalAlignment.Center;
				ResultText.Value = ResultString;
				ResultText.SetFont(Application.ResourceCache.GetFont(Assets.Fonts.Font), Application.Graphics.Width / 20);
				Application.UI.Root.AddChild(ResultText);
			}

			Text WelcomeText = new Text();
			WelcomeText.HorizontalAlignment = HorizontalAlignment.Center;
			WelcomeText.VerticalAlignment = VerticalAlignment.Center;
			WelcomeText.Position = new IntVector2(WelcomeText.Position.X, 150);
			WelcomeText.Value = WelcomeString;
			WelcomeText.SetFont(Application.ResourceCache.GetFont(Assets.Fonts.Font), Application.Graphics.Width / 20);
			Application.UI.Root.AddChild(WelcomeText);
		}
		public void RemoveMenu()
		{
			_scene?.RemoveAllComponents();
			_scene?.RemoveAllChildren();
			Application.UI.Root.RemoveAllChildren();
		}
    }
}
