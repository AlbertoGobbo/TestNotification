﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Firebase.Iid;
using System;
using TestNotification.Droid.Services;
using TestNotification.Services;
using XamarinShortcutBadger;

namespace TestNotification.Droid
{
    [Activity(
        Label = "TestNotification",
        // This attribute indicates it is allowed only one instance of this activity
        LaunchMode = LaunchMode.SingleTop,
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges =
        ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity, Android.Gms.Tasks.IOnSuccessListener
    {
        public static bool isActivityActive = false;

        IDeviceInstallationService _deviceInstallationService;

        IDeviceInstallationService DeviceInstallationService
            => _deviceInstallationService ??
                (_deviceInstallationService =
                ServiceContainer.Resolve<IDeviceInstallationService>());


        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Bootstrap.Begin(() => new DeviceInstallationService());

            if (DeviceInstallationService.NotificationsSupported)
            {
                FirebaseInstanceId.GetInstance(Firebase.FirebaseApp.Instance)
                    .GetInstanceId()
                    .AddOnSuccessListener(this);
            }

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new App());

            CreateNotificationChannel();

            if (!ShortcutBadger.IsBadgeCounterSupported(this))
                Console.WriteLine("Pay attention: badge counter not supported");
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
        }

        protected override void OnStart()
        {
            base.OnStart();
            isActivityActive = true;
            ShortcutBadger.ApplyCount(this, 0);
        }

        protected override void OnStop()
        {
            base.OnStop();
            isActivityActive = false;
        }

        protected override void OnResume()
        {
            base.OnResume();
            isActivityActive = true;
        }

        public override void OnBackPressed()
        {
            using (var alert = new AlertDialog.Builder(this))
            {
                alert.SetTitle("Confirm Exit")
                .SetMessage("Are you sure you want to exit?")
                .SetPositiveButton("Yes", (sender, args) => { Finish(); })
                .SetNegativeButton("No", (sender, args) => { });

                var dialog = alert.Create();
                dialog.Show();
            }
        }

        // Retrieve and store the Firebase token
        public void OnSuccess(Java.Lang.Object result)
            => DeviceInstallationService.Token =
                result.Class.GetMethod("getToken").Invoke(result).ToString();

        // Notification channels are available only for API 26 and higher
        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channel = new NotificationChannel(Constants.CHANNEL_ID, Constants.CHANNEL_NAME, NotificationImportance.High)
            {
                Description = Constants.CHANNEL_DESCRIPTION
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}