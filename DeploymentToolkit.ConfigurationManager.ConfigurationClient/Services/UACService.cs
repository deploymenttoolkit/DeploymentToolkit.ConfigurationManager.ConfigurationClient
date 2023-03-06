using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public class UACService
    {
        public bool IsElevated
        {
            get
            {
                return WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        public void RestartAsAdmin()
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    FileName = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe"),
                    Verb = "runas"
                }
            };

            process.Start();
            Environment.Exit(0);
        }
    }
}
