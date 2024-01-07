using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Work;
using Java.Util.Concurrent;

namespace MobileFlat.Droid
{
    [Activity(Label = "ЖКХ", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            var workRequest = new PeriodicWorkRequest.Builder(typeof(MobileFlatWorker), 2, TimeUnit.Hours)
                .AddTag(MobileFlatWorker.TAG)
                .Build();
            WorkManager.GetInstance(Application.Context).EnqueueUniquePeriodicWork(
                MobileFlatWorker.TAG, ExistingPeriodicWorkPolicy.Keep, workRequest);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}