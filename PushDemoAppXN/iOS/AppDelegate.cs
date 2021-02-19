using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Foundation;
using PushDemoApp;
using PushDemoApp.iOS.Extensions;
using PushDemoApp.iOS.Services;
using PushDemoApp.Services;
using UIKit;
using UserNotifications;

namespace PushDemoAppXN.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        IPushDemoNotificationActionService _notificationActionService;
        INotificationRegistrationService _notificationRegistrationService;
        IDeviceInstallationService _deviceInstallationService;

        IPushDemoNotificationActionService NotificationActionService
            => _notificationActionService ?? (_notificationActionService = ServiceContainer.Resolve<IPushDemoNotificationActionService>());

        INotificationRegistrationService NotificationRegistrationService
            => _notificationRegistrationService ?? (_notificationRegistrationService = ServiceContainer.Resolve<INotificationRegistrationService>());

        IDeviceInstallationService DeviceInstallationService
            => _deviceInstallationService ?? (_deviceInstallationService = ServiceContainer.Resolve<IDeviceInstallationService>());

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Bootstrap.Begin(() => new DeviceInstallationService());

            if (DeviceInstallationService.NotificationsSupported)
            {
                UNUserNotificationCenter.Current.RequestAuthorization(
                        UNAuthorizationOptions.Alert |
                        UNAuthorizationOptions.Badge |
                        UNAuthorizationOptions.Sound,
                        (approvalGranted, error) =>
                        {
                            if (approvalGranted && error == null)
                                RegisterForRemoteNotifications();
                        });
            }

            using (var userInfo = launchOptions?.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey) as NSDictionary)
                ProcessNotificationActions(userInfo);

            ServiceContainer.Resolve<IPushDemoNotificationActionService>().ActionTriggered += AppDelegate_ActionTriggered; ;

            return true;
        }

        private void AppDelegate_ActionTriggered(object sender, PushDemoApp.Models.PushDemoAction e)
        {
            var pushView = UIAlertController.Create("Notification received", $"{e} received", UIAlertControllerStyle.Alert);
            pushView.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, alert => Console.WriteLine("Push message, ok button was clicked")));
            Window.MakeKeyAndVisible();
            this.Window.RootViewController.PresentViewController(pushView, true, null);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            CompleteRegistrationAsync(deviceToken).ContinueWith((task) => { if (task.IsFaulted) throw task.Exception; });
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            ProcessNotificationActions(userInfo);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            Debug.WriteLine(error.Description);
        }

        void RegisterForRemoteNotifications()
        {
            InvokeOnMainThread(() =>
            {
                /*var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert |
                    UIUserNotificationType.Badge |
                    UIUserNotificationType.Sound,
                    new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);*/
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            });
        }

        Task CompleteRegistrationAsync(NSData deviceToken)
        {
            DeviceInstallationService.Token = deviceToken.ToHexString();
            return NotificationRegistrationService.RefreshRegistrationAsync();
        }

        void ProcessNotificationActions(NSDictionary userInfo)
        {
            if (userInfo == null)
                return;

            try
            {
                var actionValue = userInfo.ObjectForKey(new NSString("action")) as NSString;

                if (!string.IsNullOrWhiteSpace(actionValue?.Description))
                    NotificationActionService.TriggerAction(actionValue.Description);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

