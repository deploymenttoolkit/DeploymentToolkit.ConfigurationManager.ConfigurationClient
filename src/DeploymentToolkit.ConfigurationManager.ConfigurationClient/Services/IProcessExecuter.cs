
namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public interface IProcessExecuter
    {
        public bool TryExecute(string filePath, string? arguments, out string output, int timeout = 30000);
    }
}
