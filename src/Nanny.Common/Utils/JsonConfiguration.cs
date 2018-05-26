using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nanny.Common.Utils
{
    public class JsonConfiguration
    {
        private IDictionary<string, string> Configuration { get; }

        private JsonConfiguration(IDictionary<string, string> configuration)
        {
            Configuration = configuration;
        }

        public string this[string key] => Configuration[key];


        public static JsonConfiguration Build(FileInfo jsonFile)
        {
            if (!jsonFile.Exists) throw new FileNotFoundException(nameof(jsonFile), jsonFile.FullName);

            var json = File.ReadAllText(jsonFile.FullName);
            var obj = JObject.Parse(json);

            var dictionary = obj.Descendants()
                .OfType<JProperty>()
                .Select(p => new KeyValuePair<string, object>(p.Path,
                    p.Value.Type == JTokenType.Array || p.Value.Type == JTokenType.Object ? null : p.Value)
                ).ToDictionary(kp => kp.Key, kp => kp.Value.ToString());

            return new JsonConfiguration(dictionary);
        }

        public static JsonConfiguration Build(string jsonFilePath) => Build(new FileInfo(jsonFilePath));
    }
}
