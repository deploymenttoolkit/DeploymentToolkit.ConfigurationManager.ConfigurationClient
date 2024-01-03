using System;
using System.IO;
using System.Threading.Tasks;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Settings;
using Microsoft.Extensions.Logging;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
public class LocalSettingsService
{
    public UserSettings UserSettings { get; private set; }

    private const string _defaultApplicationDataFolder = "DeploymentToolkit/ConfigurationManager.ConfigurationClient/Config";
    private const string _defaultLocalSettingsFile = "LocalSettings.json";

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _localApplicationPath;
    private readonly string _localsettingsFile;

    private readonly ILogger _logger;

    public LocalSettingsService(ILogger<LocalSettingsService> logger)
    {
        _logger = logger;

        _localApplicationPath = Path.Combine(_localApplicationData, _defaultApplicationDataFolder);
        _localsettingsFile = Path.Combine(_localApplicationPath, _defaultLocalSettingsFile);

        UserSettings = LoadSettingsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<UserSettings> LoadSettingsAsync()
    {
        if(!Directory.Exists(_localApplicationPath) || !File.Exists(_localsettingsFile))
        {
            return new();
        }

        try
        {
            var text = File.ReadAllText(_localsettingsFile);
            var result = System.Text.Json.JsonSerializer.Deserialize<UserSettings>(text);
            // Doing it async throws no exception for me but also doesn't convert shit
            // I don't know whats happening here
            //using var stream = new FileStream(_localsettingsFile, FileMode.Open, FileAccess.ReadWrite);
            //var result = await System.Text.Json.JsonSerializer.DeserializeAsync<UserSettings>(stream);
            return result ?? throw new Exception("Failed to parse");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to load settings from {path}", _localsettingsFile);
            return new();
        }
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            if (!Directory.Exists(_localApplicationPath))
            {
                Directory.CreateDirectory(_localApplicationPath);
            }

            using var stream = new FileStream(_localsettingsFile, FileMode.OpenOrCreate);
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, UserSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings to {localPath}", _localsettingsFile);
        }
    }
}
