using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Messages
{
    public class CCMApplicationMessage : ValueChangedMessage<InstanceEvent>
    {
        public CCMApplicationMessage(InstanceEvent value) : base(value)
        {
        }
    }
}
