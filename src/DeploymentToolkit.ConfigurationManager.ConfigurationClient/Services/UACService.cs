using CCMEXEC;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Management;
using System.Reflection;
using System.Security.Principal;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;

public class UACService(ILogger<UACService> logger)
{
    public bool IsElevated { get; private set; }

    private readonly ILogger<UACService> _logger = logger;

    public void UpdateAdminStatus(IWindowsManagementInstrumentationConnection? windowsManagementInstrumentastionConnection)
    {
        if(windowsManagementInstrumentastionConnection == null)
        {
            IsElevated = WindowsIdentity.GetCurrent().Owner!.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            return;
        }
        
        if(!windowsManagementInstrumentastionConnection.IsConnected)
        {
            IsElevated = false;
            return;
        }

        try
        {
            windowsManagementInstrumentastionConnection.GetChildClasses(new DynamicWMIClass(@"ROOT\SECURITY", null!));

            IsElevated = true;
        }
        catch(Exception ex) when (ex is ManagementException || ex is ManagementException)
        {
            IsElevated = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unhandeled error while checking for permission");
        }
    }

    public void RestartAsAdmin()
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = Assembly.GetEntryAssembly()!.Location.Replace(".dll", ".exe"),
                Verb = "runas"
            }
        };

        process.Start();
        Environment.Exit(0);
    }
}
