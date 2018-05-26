using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nanny.Queue.Clients;
using Nanny.Queue.Models;
using Nanny.Queue.Utils;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nanny.Queue.Tests
{
    [TestClass]
    public class ConnectionStringTest
    {
        string connectionString = "Endpoint=sb://YOURSERVICEBUS.servicebus.windows.net/;SharedAccessKeyName=YOURSASNAME;SharedAccessKey=YOURSASKEY";
        string queueName = "YOURQUEUE";

        [TestMethod]
        public void ParseConnectionString()
        {
            ServiceBusConnectionValues values = ServiceBusConnectionValues.CreateFromConnectionString(connectionString);

            Assert.AreEqual(values.ServiceBusEndpoint, "https://YOURSERVICEBUS.servicebus.windows.net/", true);
            Assert.AreEqual(values.SasKeyName, "YOURSASNAME", false);
            Assert.AreEqual(values.SasKeyValue, "YOURSASKEY", false);
        }

        [TestMethod]
        public void BuildToken()
        {
            ServiceBusConnectionValues values = ServiceBusConnectionValues.CreateFromConnectionString(connectionString);

            TimeSpan validFor = new TimeSpan(1,0,0);
            DateTime expiresOn = DateTime.Now.Add(validFor);

            ServiceBusToken token = ServiceBusTokenManager.GetToken(values, validFor.TotalSeconds);

            Assert.AreEqual(token.IsExpired, false);
            Assert.AreEqual(token.ExpiresOn > expiresOn, true);
            Assert.AreNotEqual(token.Value, string.Empty);

            Trace.WriteLine(token.Value);
        }

        [TestMethod]
        public async Task GetMessageCount()
        {
            //TODO: Refactor this method
            //ServiceBusQueueClient client = new ServiceBusQueueClient(connectionString);

            //var messages = await client.GetMessageCountAsync(queueName);

            //Assert.AreEqual(messages >= 0, true);

            //Trace.WriteLine(messages);
        }
    }
}
