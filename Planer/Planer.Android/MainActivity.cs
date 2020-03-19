using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Urho.Droid;

namespace Planer.Droid
{
    [Activity(Label = "Planer", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var mLayout = new AbsoluteLayout(this);
            var surface = UrhoSurface.CreateSurface(this);// (this, , true);
            mLayout.AddView(surface);
            SetContentView(mLayout);
            var app = await surface.Show<GameSession>(new Urho.ApplicationOptions("Data"));
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}