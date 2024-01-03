using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views
{
    public sealed partial class GeneralPage : Page
    {
        public GeneralPageViewModel ViewModel { get; set; }

        public GeneralPage()
        {
            this.InitializeComponent();
            this.ViewModel = App.Current.Services.GetService<GeneralPageViewModel>();
        }
    }
}
