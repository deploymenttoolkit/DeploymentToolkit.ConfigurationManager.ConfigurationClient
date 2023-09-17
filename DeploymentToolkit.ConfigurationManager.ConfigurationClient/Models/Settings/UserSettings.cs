using System.Collections.Generic;
using Microsoft.UI.Xaml;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Settings;
public class UserSettings
{
    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    public Dictionary<string, object> WinUIExPersistence = new();
}
