using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushDemoApp.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PushDemoApp
{
    public partial class MainPage : ContentPage
    {
        readonly INotificationRegistrationService _notificationRegistrationService;

        public MainPage()
        {
            InitializeComponent();

            _notificationRegistrationService = ServiceContainer.Resolve<INotificationRegistrationService>();
        }

        void RegisterButton_Clicked(System.Object sender, System.EventArgs e)
        {
            _notificationRegistrationService.RegisterDeviceAsync().ContinueWith((task)
                => { ShowAlert(task.IsFaulted ? task.Exception.Message : "Device registered"); });
        }

        void DeregisterButton_Clicked(System.Object sender, System.EventArgs e)
        {
            _notificationRegistrationService.DeregisterDeviceAsync().ContinueWith((task)
                => { ShowAlert(task.IsFaulted ? task.Exception.Message : "Device deregistered"); });
        }

        void ShowAlert(string message)
            => MainThread.BeginInvokeOnMainThread(() => DisplayAlert("PushDemo", message, "OK").ContinueWith((task)
            => { if (task.IsFaulted) throw task.Exception; }));
    }
}
