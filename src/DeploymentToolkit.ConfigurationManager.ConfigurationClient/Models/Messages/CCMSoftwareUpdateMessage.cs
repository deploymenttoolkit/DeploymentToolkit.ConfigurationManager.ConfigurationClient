using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages;

public class CCMSoftwareUpdateMessage : ValueChangedMessage<InstanceEvent>
{
    public CCMSoftwareUpdateMessage(InstanceEvent value) : base(value)
    {
    }
}
