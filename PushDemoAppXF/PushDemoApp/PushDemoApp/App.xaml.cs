using System;
using PushDemoApp.Models;
using PushDemoApp.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PushDemoApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            ServiceContainer.Resolve<IPushDemoNotificationActionService>().ActionTriggered += App_ActionTriggered;

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        void App_ActionTriggered(object sender, PushDemoAction e) => ShowActionAlert(e);

        void ShowActionAlert(PushDemoAction action)
            => MainThread.BeginInvokeOnMainThread(()
                => MainPage?.DisplayAlert("PushDemo", $"{action} action received", "OK")
                    .ContinueWith((task) => { if (task.IsFaulted) throw task.Exception; }));
    }
}
