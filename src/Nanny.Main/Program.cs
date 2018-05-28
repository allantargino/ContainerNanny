using Nanny.Common.Interfaces;
using Nanny.Kubernetes;
using Nanny.Main.Configuration;
using Nanny.Main.Factories;
using Nanny.Main.Rules;
using Nanny.Queue.Clients;
using System;
using System.Threading.Tasks;

namespace Nanny.Main
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var configurationManager = new ConfigurationManager(args);
            var configuration = ConfigurationValues.GetFromConfigurationManager(configurationManager);

            IQueueClient queueClient = QueueClientFactory.GetQueueClientFromConnectionString(configuration.Queue.ConnectionString);

            KubeClient kubeClient;
            if (String.IsNullOrWhiteSpace(configuration.Kubernetes.KubeConfig))
                kubeClient = new KubeClient();
            else
                kubeClient = new KubeClient(configuration.Kubernetes.KubeConfig);

            IScalingRule scalingRule = new IncrementRule(new TimeSpan(0, 1, 0));

            await CheckQueue(queueClient, kubeClient, scalingRule, configuration, configurationManager);
        }

        static async Task CheckQueue(IQueueClient queueClient, KubeClient kubeClient, IScalingRule scalingRule, ConfigurationValues configuration, ConfigurationManager configurationManager)
        {
            Console.WriteLine("Nanny.Checker is Running.");

            var queueName = configuration.Queue.QueueName;

            do
            {
                if (configurationManager.GetNotRequired("NANNY_IS_PAUSED", false))
                {
                    var nextCheck = 60;
                    Console.WriteLine($"Waiting for {nextCheck} seconds, before checking queue {queueName}");
                    await Task.Delay(nextCheck * 1000);
                }

                var messageCount = await queueClient.GetMessageCountAsync(queueName);
                Console.WriteLine($"There are {messageCount} messages in the queue {queueName}");

                var currentRunningJobs = await GetCurrentRunningJobs(kubeClient, configuration.Kubernetes.K8Namespace, queueName);
                Console.WriteLine($"I have found {messageCount} jobs in Kubernetes");

                if (await kubeClient.IsResourceAvailableAsync() && currentRunningJobs < configuration.Kubernetes.ContainerLimit)
                {
                    var result = scalingRule.GetJobScalingResult(messageCount, currentRunningJobs);

                    if (result.JobCount > 0)
                    {
                        Console.WriteLine($"Creating {result.JobCount} jobs");
                        await CreateKubeJob(kubeClient, configuration.Kubernetes, queueName, result.JobCount);
                    }
                    Console.WriteLine($"Waiting for {result.NextCheck.TotalSeconds} seconds, before checking queue {queueName}");
                    await Task.Delay(result.NextCheck);
                }
                else
                {
                    Console.WriteLine("No more resources available in the cluster!");
                    await Task.Delay(5 * 60 * 1000);
                }
            } while (true);
        }

        static async Task CreateKubeJob(KubeClient kubeClient, ConfigurationKubernetes configuration, string label, int jobCount = 1)
        {
            if (jobCount <= 0) return;

            var configMap = await kubeClient.GetConfigMapListAsync(configuration.K8Namespace, configuration.JobConfigMapName);

            if (configMap == null)
            {
                throw new ApplicationException($"Configuration Map '{configuration.JobConfigMapName}' for the nanny queue");
            }

            var job_name = Guid.NewGuid().ToString();
            Console.WriteLine($"Job {job_name} has been created!");
            var job = await kubeClient.CreateJobAsync(job_name, jobCount, 1, configuration.ContainerName, configuration.ContainerImage, configuration.K8Secret, label, configMap.Data, configuration.K8Namespace, configuration.JobCpuRequest, configuration.JobMemRequest, configuration.JobCpuLimit, configuration.JobMemLimit);
        }

        static async Task<int> GetCurrentRunningJobs(KubeClient kubeClient, string _namespace, string labelSelector = "")
        {
            return await kubeClient.GetActivePodCountFromNamespaceAsync(_namespace, labelSelector);
        }
    }
}
