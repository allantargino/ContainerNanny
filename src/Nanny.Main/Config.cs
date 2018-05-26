using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace Nanny.Main
{
    /// <summary>
    /// This class put together all sources of configuration: Files, Environment Variables and Commnand Args
    /// 
    /// And it respect the prevalence order:
    /// Command Args -> Environment Variables -> Files
    /// </summary>
    public class Config
    {
        private readonly IConfigurationRoot _config;

        public Config(string[] args, string env = "prod")
        {
            _config = new ConfigurationBuilder()
                            .AddJsonFile("settings.json", true, true)
                            .AddJsonFile($"settings.{env}.json", true, true)
                            .AddEnvironmentVariables()
                            .AddCommandLine(args)
                            .Build();
        }

        [DebuggerHidden]
        public string Get(string configName)
        {
            var value = _config[configName];

            if (value == null)
                throw new NotConfigured(configName);

            return value;
        }


        class NotConfigured : Exception
        {
            public readonly string Name;

            public NotConfigured(string name) : base($"configuration '{name}' not found!")
            {
                Name = name;
            }
        }

    }
}
