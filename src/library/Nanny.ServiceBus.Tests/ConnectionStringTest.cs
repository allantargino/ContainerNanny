using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nanny.ServiceBus.Tests
{
    [TestClass]
    public class ConnectionStringTest
    {
        string connectionString = "Endpoint=sb://YOURSERVICEBUS.servicebus.windows.net/;SharedAccessKeyName=YOURSASNAME;SharedAccessKey=YOURSASKEY";
        string queueName = "YOURQUEUE";

        [TestMethod]
        public void ParseConnectionString()
        {
            ConnectionStringValues values = ConnectionStringValues.CreateFromConnectionString(connectionString);

            Assert.AreEqual(values.ServiceBusEndpoint, "https://YOURSERVICEBUS.servicebus.windows.net/", true);
            Assert.AreEqual(values.SasKeyName, "YOURSASNAME", false);
            Assert.AreEqual(values.SasKeyValue, "YOURSASKEY", false);
        }

        [TestMethod]
        public void BuildToken()
        {
            ConnectionStringValues values = ConnectionStringValues.CreateFromConnectionString(connectionString);

            TimeSpan validFor = new TimeSpan(1,0,0);
            DateTime expiresOn = DateTime.Now.Add(validFor);

            Token token = TokenManager.GetToken(values, validFor.TotalSeconds);

            Assert.AreEqual(token.IsExpired, false);
            Assert.AreEqual(token.ExpiresOn > expiresOn, true);
            Assert.AreNotEqual(token.Value, string.Empty);

            Trace.WriteLine(token.Value);
        }

        [TestMethod]
        public async Task GetMessageCount()
        {
            ServiceBusQueueClient client = new ServiceBusQueueClient(connectionString);

            var messages = await client.GetMessageCountAsync(queueName);

            Assert.AreEqual(messages >= 0, true);

            Trace.WriteLine(messages);
        }
    }
}
