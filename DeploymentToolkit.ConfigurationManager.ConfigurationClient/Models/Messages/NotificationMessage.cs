using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages;

public class NotificationMessage : ValueChangedMessage<InAppNotificationData>
{
    public NotificationMessage(InAppNotificationData value) : base(value)
    {
    }
}