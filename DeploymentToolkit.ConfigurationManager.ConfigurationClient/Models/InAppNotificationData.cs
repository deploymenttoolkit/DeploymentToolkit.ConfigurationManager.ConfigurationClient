using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class InAppNotificationData : ObservableObject
    {
        [ObservableProperty]
        private string _title;
        [ObservableProperty]
        private string _message;

        [ObservableProperty]
        private InfoBarSeverity _severity;

        public InAppNotificationData(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
        {
            Title = title;
            Message = message;
            Severity = severity;
        }
    }
}
