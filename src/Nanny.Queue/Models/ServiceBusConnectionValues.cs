using System;

namespace Nanny.Queue.Models
{
    public class ServiceBusConnectionValues
    {
        public string ServiceBusEndpoint { get; }
        public string SasKeyName { get; }
        public string SasKeyValue { get; }

        private ServiceBusConnectionValues(string endpoint, string sasKeyName, string sasKeyValue)
        {
            if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException(nameof(endpoint));
            if (string.IsNullOrEmpty(sasKeyName)) throw new ArgumentNullException(nameof(sasKeyName));
            if (string.IsNullOrEmpty(sasKeyValue)) throw new ArgumentNullException(nameof(sasKeyValue));

            ServiceBusEndpoint = endpoint;
            SasKeyName = sasKeyName;
            SasKeyValue = sasKeyValue;
        }

        public static ServiceBusConnectionValues CreateFromConnectionString(string connectionString)
        {
            var endpoint = string.Empty;
            var sasKeyName = string.Empty;
            var sasKeyValue = string.Empty;

            string[] parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("Endpoint"))
                    endpoint = "https" + part.Substring(11);
                if (part.StartsWith("SharedAccessKeyName"))
                    sasKeyName = part.Substring(20);
                if (part.StartsWith("SharedAccessKey"))
                    sasKeyValue = part.Substring(16);
            }

            return new ServiceBusConnectionValues(endpoint, sasKeyName, sasKeyValue);
        }
    }
}
