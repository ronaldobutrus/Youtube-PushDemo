using System;
namespace PushDemoApp.Services
{
    public interface INotificationActionService
    {
        void TriggerAction(string action);
    }
}
