using System;
using System.Threading.Tasks;
using PushDemoApp.Services;
using UIKit;

namespace PushDemoAppXN.iOS
{
    public partial class ViewController : UIViewController
    {
        readonly INotificationRegistrationService _notificationRegistrationService;

        public ViewController(IntPtr handle) : base(handle)
        {
            _notificationRegistrationService = ServiceContainer.Resolve<INotificationRegistrationService>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            registerButton.TouchUpInside += delegate
            {
                Task task = _notificationRegistrationService.RegisterDeviceAsync(tagTextField.Text);

                var okAlertController = UIAlertController.Create("Registration",
                    task.IsFaulted ? task.Exception.Message : "Device registered",
                    UIAlertControllerStyle.Alert);

                okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

                PresentViewController(okAlertController, true, null);
            };

            deregisterButton.TouchUpInside += delegate
            {
                Task task = _notificationRegistrationService.DeregisterDeviceAsync();

                var okAlertController = UIAlertController.Create("Deregistration",
                    task.IsFaulted ? task.Exception.Message : "Device deregistered",
                    UIAlertControllerStyle.Alert);

                okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

                PresentViewController(okAlertController, true, null);
            };
        }
    }
}
