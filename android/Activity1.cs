using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace Minlicon.Android;

[Activity(
    Label = "@string/app_name",
    MainLauncher = true,
    Icon = "@drawable/icon",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.Landscape,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
)]
public class Activity1 : AndroidGameActivity {
    private Minlicon.Core.Main _game;
    private View _view;

    protected override void OnCreate(Bundle bundle) {
        base.OnCreate(bundle);

        _game = new Minlicon.Core.Main();
        _view = _game.Services.GetService(typeof(View)) as View;

        SetContentView(_view);
        _game.Run();
    }
}