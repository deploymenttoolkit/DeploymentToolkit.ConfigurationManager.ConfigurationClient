using Microsoft.UI.Xaml.Controls;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WinRMDebugPage : Page
    {
        public WinRMDebugPageViewModel ViewModel { get; set; }

        public WinRMDebugPage()
        {
            ViewModel = App.Current.Services.GetService<WinRMDebugPageViewModel>();
            this.InitializeComponent();
        }
    }
}
