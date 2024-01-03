namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.cimv2;

public class Win32_ProcessStartup : IWSManAdvancedParameter
{
    public ushort ShowWindow { get; set; }

    public string ToXml(string prefix)
    {
        return $@"
<{prefix}:Win32_ProcessStartup_INPUT xmlns:{prefix}=""http://schemas.microsoft.com/wbem/wsman/1/wmi/root/cimv2/Win32_ProcessStartup"" xmlns:cim=""http://schemas.dmtf.org/wbem/wscim/1/common"">
<{prefix}:ShowWindow><cim:uint16>{ShowWindow}</cim:uint16></{prefix}:ShowWindow>
</{prefix}:Win32_ProcessStartup_INPUT>";
    }
}
