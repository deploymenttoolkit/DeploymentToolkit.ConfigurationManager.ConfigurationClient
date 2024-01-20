using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels.SoftwareCenter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views.SoftwareCenter;

public sealed partial class ApplicationsPage : Page
{
    public ApplicationsPageViewModel ViewModel;

    public ApplicationsPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<ApplicationsPageViewModel>()!;
    }
}
