using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nanny.Common.Utils;

namespace Nanny.Common.Tests
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void BuildJson()
        {
            var settings = JsonConfiguration.Build("./settings.json");

            Assert.IsNotNull(settings);
            Assert.AreEqual("Endpoint=sb://YOURSERVICEBUS.servicebus.windows.net/;SharedAccessKeyName=YOURSASNAME;SharedAccessKey=YOURSASKEY", settings["ServiceBusConnectionString"]);
            Assert.AreEqual("YOURQUEUE", settings["ServiceBusQueueName"]);
            Assert.AreEqual("2500", settings["ServiceBusMessageDelay"]);
            Assert.AreEqual("./config", settings["KubernetesConfig"]);
        }
    }
}
