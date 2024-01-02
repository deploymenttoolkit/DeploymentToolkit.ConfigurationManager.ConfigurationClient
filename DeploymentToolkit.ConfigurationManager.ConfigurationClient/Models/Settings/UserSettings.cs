using System.Collections.Generic;
using Microsoft.UI.Xaml;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Settings;
public class UserSettings
{
    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    public ConnectionMethod DefaultConnectionMethod { get; set; } = ConnectionMethod.WinRM;
    public FileConnectionMethods DefaultFileConnectionMethod { get; set; } = FileConnectionMethods.Windows;

    public Dictionary<string, object> WinUIExPersistence = new();
}
