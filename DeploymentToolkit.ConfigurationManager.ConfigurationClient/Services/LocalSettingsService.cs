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
        if(!File.Exists(_localsettingsFile))
        {
            return new();
        }

        using var stream = new FileStream(_localsettingsFile, FileMode.Open);
        return await System.Text.Json.JsonSerializer.DeserializeAsync<UserSettings>(stream) ?? new();
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
