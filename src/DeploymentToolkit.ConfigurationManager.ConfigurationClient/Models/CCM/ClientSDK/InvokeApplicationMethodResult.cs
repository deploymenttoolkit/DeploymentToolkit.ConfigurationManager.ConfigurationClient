namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;

public class InvokeApplicationMethodResult : IMethodResult
{
    public uint ReturnValue { get; set; }
    public string JobId { get; set; }
}
