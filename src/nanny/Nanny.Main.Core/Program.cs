using Nanny.Common.Interfaces;
using Nanny.Common.Models;
using Nanny.Common.Utils;
using Nanny.Kubernetes;
using Nanny.Main.Core.Rules;
using Nanny.ServiceBus;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nanny.Main.Core
{
    public class Program
    {
        static JsonConfiguration settings;
        static KubeClient kube;
        static ServiceBusQueueClient sb;
        static IScalableRule rule;

        static async Task Main(string[] args)
        {
            settings = JsonConfiguration.Build("./settings.json");

            kube = new KubeClient(settings["KubernetesConfig"]);
            sb = new ServiceBusQueueClient(settings["ServiceBusConnectionString"]);

            rule = new IncrementRule(new TimeSpan(0, 0, 10)); // Scaling rule

            await CheckQueue(rule);
        }

        static async Task CheckQueue(IScalableRule rule)
        {
            Console.WriteLine("Nanny.Checker is Running.");

            do
            {
                var messageCount = await GetMessageCount();
                Console.WriteLine($"There are {messageCount} messages in Service Bus");

                var currentRunningJobs = await GetCurrentRunningJobs();
                Console.WriteLine($"I have found {messageCount} jobs in Kubernetes");

                var result = rule.GetJobScalingResult(messageCount, currentRunningJobs);

                if (result.JobCount > 0)
                {
                    Console.WriteLine($"Creating {result.JobCount} jobs");
                    await CreateKubeJob(result.JobCount);
                }

                Console.WriteLine($"Waiting for {result.NextCheck.TotalSeconds} seconds, before checking Service Bus");
                await Task.Delay(result.NextCheck);

            } while (true);
        }

        #region Util

        static async Task<long> GetMessageCount()
        {
            return await sb.GetMessageCountAsync(settings["ServiceBusQueueName"]);
        }

        static async Task CreateKubeJob(int jobCount = 1)
        {
            if (jobCount <= 0) return;

            var job_name = Guid.NewGuid().ToString();
            Console.WriteLine($"Job {job_name} has been created!");
            var job = await kube.CreateJobAsync(job_name, jobCount, jobCount, "queueconsumer", "twgnanny.azurecr.io/app/queueconsumer", "acrkey");
        }

        static async Task<int> GetCurrentRunningJobs(string _namespace = "default")
        {
            return await kube.GetActivePodCountFromNamespaceAsync(_namespace);
        }

        #endregion
    }
}
