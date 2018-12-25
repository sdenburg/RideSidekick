using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;

namespace RideSidekick.Droid
{
    [Activity(Label = "RideSidekick", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            this.CheckPermissions();

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Xamarin.FormsMaps.Init(this, savedInstanceState);

            LoadApplication(new App());
        }

        private void CheckPermissions()
        {
            var permissions = new [] {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.AccessLocationExtraCommands,
                Manifest.Permission.AccessMockLocation,
                Manifest.Permission.AccessNetworkState,
                Manifest.Permission.AccessWifiState, 
                Manifest.Permission.Internet
            };

            this.RequestPermissions(permissions, 1);
        }
    }
}