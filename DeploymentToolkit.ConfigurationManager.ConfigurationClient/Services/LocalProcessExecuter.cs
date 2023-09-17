using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public class LocalProcessExecuter : IProcessExecuter
    {
        private readonly ILogger<LocalProcessExecuter> _logger;

        public LocalProcessExecuter(ILogger<LocalProcessExecuter> logger)
        {
            _logger = logger;
        }

        public bool TryExecute(string filePath, string? arguments, out string output, int timeout = 30000)
        {
            if(!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = filePath,
                    Arguments = arguments,

                    UseShellExecute = false,
                    LoadUserProfile = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,

                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            _logger.LogDebug("Starting {filePath}", filePath);
            if (arguments != null)
            {
                _logger.LogTrace("Arguments {arguments}", arguments);
            }

            process.Start();

            if(!process.WaitForExit(timeout))
            {
                _logger.LogError("Failed to wait for process to finish after {seconds} seconds. Killing process", timeout / 1000);
                process.Kill();
                output = string.Empty;
                return false;
            }

            output = process.StandardOutput.ReadToEnd();
            if(!process.StandardError.EndOfStream)
            {
                output = process.StandardError.ReadToEnd() + Environment.NewLine + output;
            }

            return true;
        }
    }
}
