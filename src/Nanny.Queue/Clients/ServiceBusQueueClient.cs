using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nanny.Common.Interfaces;
using Nanny.Common.Utils;
using Nanny.Queue.Models;
using Nanny.Queue.Utils;

namespace Nanny.Queue.Clients
{
    public class ServiceBusQueueClient : IQueueClient
    {
        private ServiceBusConnectionValues ConnectionValues { get; }
        private Token Token { get; set; }

        public ServiceBusQueueClient(ServiceBusConnectionValues connectionValues)
        {
            ConnectionValues = connectionValues;
            Token = GetNewToken();
        }

        public ServiceBusQueueClient(string connectionString)
            : this(
                    ServiceBusConnectionValues.CreateFromConnectionString(connectionString)
                 )
        { }

        private Token GetNewToken() => ServiceBusTokenManager.GetToken(ConnectionValues);

        public async Task<long> GetMessageCountAsync(string queueName)
        {
            if (Token.IsExpired)
                Token = GetNewToken();

            return await GetQueueMessages(ConnectionValues.ServiceBusEndpoint, queueName, Token);
        }

        private async Task<long> GetQueueMessages(string serviceBusUrl, string queueName, Token token)
        {
            var apiVersion = "2017-04";
            var queueEndpoint = $"{serviceBusUrl}{queueName}/?api-version={apiVersion}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", token.Value);
                var xmlResponse = await client.RestGet(queueEndpoint);

                var messageCount = new Regex(@"<MessageCount>(.+?)<\/MessageCount>").Match(xmlResponse).Groups[1].Value;
                return long.Parse(messageCount);
            }
        }
    }
}
