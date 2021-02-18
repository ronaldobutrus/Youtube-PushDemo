using System;
using PushDemoApp.Models;

namespace PushDemoApp.Services
{
    public interface IPushDemoNotificationActionService : INotificationActionService
    {
        event EventHandler<PushDemoAction> ActionTriggered;
    }
}
