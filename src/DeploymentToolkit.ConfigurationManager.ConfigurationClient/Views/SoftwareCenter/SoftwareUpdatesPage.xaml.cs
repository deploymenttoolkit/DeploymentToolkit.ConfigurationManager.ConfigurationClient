using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels.SoftwareCenter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views.SoftwareCenter;

public sealed partial class SoftwareUpdatesPage : Page
{
    public SoftwareUpdatesPageViewModel ViewModel { get; set; }

    public SoftwareUpdatesPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<SoftwareUpdatesPageViewModel>()!;
    }
}
