using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PushDemoAPI.Models;

namespace PushDemoAPI.Services
{
    public class NotificationHubService : INotificationService
    {
        readonly NotificationHubClient _hub;
        readonly Dictionary<string, NotificationPlatform> _installationPlatform;
        readonly ILogger<NotificationHubService> _logger;

        public NotificationHubService(IOptions<NotificationHubOptions> options, ILogger<NotificationHubService> logger)
        {
            _logger = logger;
            _hub = NotificationHubClient.CreateClientFromConnectionString(options.Value.ConnectionString, options.Value.Name);

            _installationPlatform = new Dictionary<string, NotificationPlatform>
            {
                { nameof(NotificationPlatform.Apns).ToLower(), NotificationPlatform.Apns },
                { nameof(NotificationPlatform.Fcm).ToLower(), NotificationPlatform.Fcm }
            };
        }

        public async Task<bool> CreateOrUpdateInstallationAsync(DeviceInstallation deviceInstallation, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(deviceInstallation?.InstallationId) ||
                string.IsNullOrWhiteSpace(deviceInstallation?.PushChannel) ||
                string.IsNullOrWhiteSpace(deviceInstallation?.Platform))
                return false;

            var installation = new Installation()
            {
                InstallationId = deviceInstallation.InstallationId,
                PushChannel = deviceInstallation.PushChannel,
                Tags = deviceInstallation.Tags
            };

            if (_installationPlatform.TryGetValue(deviceInstallation.Platform, out var platform))
                installation.Platform = platform;
            else
                return false;

            try
            {
                await _hub.CreateOrUpdateInstallationAsync(installation, token);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteInstallationByIdAsync(string installationId, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(installationId))
                return false;

            try
            {
                await _hub.DeleteInstallationAsync(installationId, token);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> RequestNotificationAsync(NotificationRequest notificationRequest, CancellationToken token)
        {
            if ((notificationRequest.Silent && string.IsNullOrWhiteSpace(notificationRequest?.Action)) ||
                (!notificationRequest.Silent && (string.IsNullOrWhiteSpace(notificationRequest?.Text)) ||
                string.IsNullOrWhiteSpace(notificationRequest?.Action)))
                return false;

            var androidPushTemplate = notificationRequest.Silent ? PushTemplates.Silent.Android : PushTemplates.Generic.Android;
            var iOSPushTemplate = notificationRequest.Silent ? PushTemplates.Silent.iOS : PushTemplates.Generic.iOS;

            var androidPayload = PrepareNotificationPayload(androidPushTemplate, notificationRequest);
            var iOSPayload = PrepareNotificationPayload(iOSPushTemplate, notificationRequest);

            try
            {
                if (notificationRequest.Tags.Length == 0)
                    await SendPlatformNotificationAsync(androidPayload, iOSPayload, token);
                else if(notificationRequest.Tags.Length <= 20)
                    await SendPlatformNotificationAsync(androidPayload, iOSPayload, notificationRequest.Tags, token);
                else
                {
                    var notificationTasks = notificationRequest.Tags
                        .Select((value, index) => (value, index))
                        .GroupBy(g => g.index / 20, i=>i.value)
                        .Select(tags => SendPlatformNotificationAsync(androidPayload, iOSPayload, notificationRequest.Tags, token));

                    await Task.WhenAll(notificationTasks);
                }

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending notifications");
                return false;
            }
        }

        private Task SendPlatformNotificationAsync(string androidPayload, string iOSPayload, CancellationToken token)
        {
            var sendTasks = new Task[]
            {
                _hub.SendFcmNativeNotificationAsync(androidPayload, token),
                _hub.SendAppleNativeNotificationAsync(iOSPayload, token)
            };

            return Task.WhenAll(sendTasks);
        }

        private Task SendPlatformNotificationAsync(string androidPayload, string iOSPayload, string[] tags, CancellationToken token)
        {
            var sendTasks = new Task[]
            {
                _hub.SendFcmNativeNotificationAsync(androidPayload, tags, token),
                _hub.SendAppleNativeNotificationAsync(iOSPayload, tags, token)
            };

            return Task.WhenAll(sendTasks);
        }

        private string PrepareNotificationPayload(string template, NotificationRequest request)
        {
            var payload = template
                .Replace("$(alertMessage)", request.Text, StringComparison.InvariantCulture)
                .Replace("$(alertAction)", request.Action, StringComparison.InvariantCulture);

            return payload;
        }
    }
}
