namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI
{
    public interface IWindowsManagementInstrumentationStaticInstance
    {
        public string Namespace { get; }
        public string Class { get; }
    }
}
