using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CacheDeleteDialog : Page
    {
        public bool DeletePersistentContent { get; set; }

        public CacheDeleteDialog()
        {
            this.InitializeComponent();
        }
    }
}
