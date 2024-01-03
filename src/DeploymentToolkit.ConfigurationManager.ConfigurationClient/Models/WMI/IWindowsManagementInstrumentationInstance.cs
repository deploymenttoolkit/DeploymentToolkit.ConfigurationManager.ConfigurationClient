namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI
{
    public interface IWindowsManagementInstrumentationInstance : IWindowsManagementInstrumentationStaticInstance
    {
        public string Key { get; }
        public bool QueryByFilter { get; }
    }
}
