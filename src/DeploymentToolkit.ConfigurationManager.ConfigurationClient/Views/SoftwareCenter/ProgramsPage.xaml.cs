using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels.SoftwareCenter;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views.SoftwareCenter;

public sealed partial class ProgramsPage : Page
{
    public ProgramPageViewModel ViewModel { get; set; }

    public ProgramsPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetService<ProgramPageViewModel>()!;
    }
}
