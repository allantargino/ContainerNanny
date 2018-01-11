using Nanny.Common.Utils;
using Nanny.Kubernetes;
using Nanny.ServiceBus;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nanny.Main
{
    class Program
    {
        static KubeClient kube;
        static JsonConfiguration settings;

        static void Main(string[] args)
        {
            settings = JsonConfiguration.Build("./settings.json");
            kube = new KubeClient(settings["KubernetesConfig"]);

            CheckQueue().Wait();
            Console.ReadLine();
        }

        static async Task CheckQueue()
        {
            long messageCount = await GetMessageCount();

            if (messageCount > 0)
                await CreateKubeJob();
        }

        private static async Task<long> GetMessageCount()
        {
            return await new
                ServiceBusQueueClient(settings["ServiceBusConnectionString"])
                .GetMessageCountAsync(settings["ServiceBusQueueName"]);
        }

        static async Task CreateKubeJob()
        {
            var job = await kube.CreateJobAsync(Guid.NewGuid().ToString(), 1, 1, "queueconsumer", "twgnanny.azurecr.io/app/queueconsumer", "acrkey");
            Console.WriteLine("The job was created!");
        }
    }
}
