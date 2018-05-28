using System;
using Microsoft.Extensions.Configuration;
using Nanny.Main.Utils;
using System.Diagnostics;

namespace Nanny.Main.Configuration
{
    /// <summary>
    /// This class put together all sources of configuration: Files, Environment Variables and Commnand Args
    /// 
    /// And it respect the prevalence order:
    /// Command Args -> Environment Variables -> Files
    /// </summary>
    public class ConfigurationManager
    {
        private readonly IConfigurationRoot _configuration;

        public ConfigurationManager(string[] args)
        {
            var builder = new ConfigurationBuilder();

            if (EnvironmentManager.IsDevelopment())
                builder.AddJsonFile("settings.json", false, true);

            _configuration = builder.AddEnvironmentVariables().AddCommandLine(args).Build();
        }

        [DebuggerHidden]
        public T GetRequired<T>(string configName)
        {
            string value = _configuration[configName];

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(configName, $"Configuration '{configName}' was not found!");

            return (T)Convert.ChangeType(value, typeof(T));
        }

        [DebuggerHidden]
        public T GetNotRequired<T>(string configName, T defaultValue)
        {
            string value = _configuration[configName];
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public string GetRequired(string configName)
            => GetRequired<string>(configName);

        public string GetNotRequired(string configName, string defaultValue)
            => GetNotRequired(configName, defaultValue);
    }
}
