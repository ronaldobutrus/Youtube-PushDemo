using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using PushDemoApp.Services;
using PushDemoApp;
using PushDemoApp.Droid.Services;
using Firebase.Messaging;
using Java.Lang;
using Android.Content;
using System.Threading.Tasks;

namespace PushDemoAppXN.Droid
{
    [Activity(Label = "pushdemoapp", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, Android.Gms.Tasks.IOnSuccessListener
    {
        INotificationRegistrationService _notificationRegistrationService;

        IPushDemoNotificationActionService _notificationActionService;
        IDeviceInstallationService _deviceInstallationService;

        IPushDemoNotificationActionService NotificationActionService
            => _notificationActionService ??
                (_notificationActionService =
                ServiceContainer.Resolve<IPushDemoNotificationActionService>());

        IDeviceInstallationService DeviceInstallationService
            => _deviceInstallationService ??
                (_deviceInstallationService =
                ServiceContainer.Resolve<IDeviceInstallationService>());

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            Bootstrap.Begin(() => new DeviceInstallationService());
            _notificationRegistrationService = ServiceContainer.Resolve<INotificationRegistrationService>();

            if (DeviceInstallationService.NotificationsSupported)
            {
                FirebaseMessaging.Instance.GetToken().AddOnSuccessListener(this);
            }
            
            SetContentView(Resource.Layout.Main);

            ProcessNotificationActions(Intent);

            Button registerButton = FindViewById<Button>(Resource.Id.registerButton);
            Button deregisterButton = FindViewById<Button>(Resource.Id.deregisterButton);

            registerButton.Click += RegisterButton_Click;
            deregisterButton.Click += DeregisterButton_Click;
        }

        private void RegisterButton_Click(object sender, System.EventArgs e)
        {
            Task task =_notificationRegistrationService.RegisterDeviceAsync(FindViewById<EditText>(Resource.Id.tagEditText).Text);
            Toast.MakeText(Application.Context, (task.IsFaulted ? task.Exception.Message : "Device registered"), ToastLength.Long).Show();
        }

        private void DeregisterButton_Click(object sender, System.EventArgs e)
        {
            Task task = _notificationRegistrationService.DeregisterDeviceAsync();
            Toast.MakeText(Application.Context, (task.IsFaulted ? task.Exception.Message : "Device deregistered"), ToastLength.Long).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            ProcessNotificationActions(intent);
        }

        public void OnSuccess(Object result)
        {
            DeviceInstallationService.Token = FirebaseMessaging.Instance.GetToken().ToString();
        }

        void ProcessNotificationActions(Intent intent)
        {
            try
            {
                if (intent?.HasExtra("action") == true)
                {
                    var action = intent.GetStringExtra("action");

                    if (!string.IsNullOrEmpty(action))
                        NotificationActionService.TriggerAction(action);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}

