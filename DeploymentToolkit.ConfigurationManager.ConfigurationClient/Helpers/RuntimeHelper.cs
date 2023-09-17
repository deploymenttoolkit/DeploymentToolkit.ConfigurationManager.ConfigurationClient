using System.Runtime.InteropServices;
using System.Text;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Helpers;

public class RuntimeHelper
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);

    private static bool? _isPackaged;
    public static bool IsPackaged
    {
        get
        {
            if (_isPackaged == null)
            {
                var length = 0;
                _isPackaged = GetCurrentPackageFullName(ref length, null) != 15700L;
            }
            return _isPackaged.Value;
        }
    }
}

