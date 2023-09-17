using System.Threading.Tasks;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Helpers;
using Microsoft.UI.Xaml;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public class ThemeSelectorService
    {
        public ElementTheme Theme { get; set; } = ElementTheme.Default;

        public async Task SetThemeAsync(ElementTheme theme)
        {
            Theme = theme;

            await SetRequestedThemeAsync();
        }

        public async Task SetRequestedThemeAsync()
        {
            if (App.Current.GetActiveWindow().Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = Theme;

                TitleBarHelper.UpdateTitleBar(Theme);
            }

            await Task.CompletedTask;
        }
    }
}
