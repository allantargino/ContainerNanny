using Nanny.Common.Interfaces;
using Nanny.Common.Models;
using Nanny.Common.Utils;
using Nanny.Kubernetes;
using Nanny.Main.Core.Rules;
using Nanny.ServiceBus;
using Nanny.StorageQueue;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nanny.Main.Core
{
    public class Program
    {
        static JsonConfiguration settings;
        static KubeClient kube;
        static IScalableRule rule;
        static IQueueClient queueClient;

        static string containerName;
        static string containerImage;
        static string k8Namespace;
        static string k8Secret;
        static int containerLimit;

        static async Task Main(string[] args)
        {
            settings = JsonConfiguration.Build("./settings.json");

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            var kubeConfig = Environment.GetEnvironmentVariable("KUBE_CONIFG");
            var queueName = Environment.GetEnvironmentVariable("QUEUE_NAME");

            containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME");
            containerImage = Environment.GetEnvironmentVariable("CONTAINER_IMAGE");
            k8Namespace = Environment.GetEnvironmentVariable("K8S_NAMESPACE");
            k8Secret = Environment.GetEnvironmentVariable("K8S_SECRET");

            try
            {
                containerLimit = int.Parse(Environment.GetEnvironmentVariable("MAX_CONTAINERS"));
            }
            catch {
                containerLimit = 5;
            }


            if (String.IsNullOrWhiteSpace(connectionString)) throw new ApplicationException("Connection String was not provided!!!");
            if (String.IsNullOrWhiteSpace(kubeConfig)) throw new ApplicationException("KubeConfig was not provided!!!");

            if (connectionString.IndexOf("AccountName") != -1) //Storage Account
                queueClient = new StorageQueueClient(connectionString);
            else
                queueClient = new ServiceBusQueueClient(connectionString); //Service Bus

            kube = new KubeClient(kubeConfig);

            rule = new IncrementRule(new TimeSpan(0, 0, 10)); // Scaling rule

            await CheckQueue(rule, queueName);
        }

        static async Task CheckQueue(IScalableRule rule, string queueName)
        {
            Console.WriteLine("Nanny.Checker is Running.");

            do
            {
                var messageCount = await GetMessageCount(queueName);
                Console.WriteLine($"There are {messageCount} messages in the queue {queueName}");

                var currentRunningJobs = await GetCurrentRunningJobs();
                Console.WriteLine($"I have found {messageCount} jobs in Kubernetes");

                if (await isResourceAvailableAsync() && currentRunningJobs < containerLimit)
                {

                    var result = rule.GetJobScalingResult(messageCount, currentRunningJobs);

                    if (result.JobCount > 0)
                    {
                        Console.WriteLine($"Creating {result.JobCount} jobs");
                        await CreateKubeJob(result.JobCount);
                    }
                    Console.WriteLine($"Waiting for {result.NextCheck.TotalSeconds} seconds, before checking queue {queueName}");
                    await Task.Delay(result.NextCheck);
                }
                else
                {
                    Console.WriteLine("No more resources available in the cluster!");
                    await Task.Delay(5*60*1000);
                }

            } while (true);
        }

        private static Task<bool> isResourceAvailableAsync()
        {
            return kube.isResourceAvailableAsync();
        }

        #region Util

        static async Task<long> GetMessageCount(string queueName)
        {
            return await queueClient.GetMessageCountAsync(queueName);
        }

        static async Task CreateKubeJob(int jobCount = 1)
        {
            if (jobCount <= 0) return;

            var job_name = Guid.NewGuid().ToString();
            Console.WriteLine($"Job {job_name} has been created!");
            var job = await kube.CreateJobAsync(job_name, jobCount, jobCount, containerName, containerImage,k8Secret, k8Namespace);
        }

        static async Task<int> GetCurrentRunningJobs(string _namespace = "default")
        {
            return await kube.GetActivePodCountFromNamespaceAsync(_namespace);
        }

        #endregion
    }
}
