using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Extensions
{
    public static class EnumExtensions
    {
        public static T ParseOrDefault<T>(string value) where T : Enum
        {
            if (Enum.TryParse(typeof(T), value, out var parsed))
            {
                return (T)parsed;
            }
            return default;
        }
    }
}
