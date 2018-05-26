using Nanny.Common.Interfaces;
using Nanny.Kubernetes;
using Nanny.Main.Rules;
using Nanny.Queue.Clients;
using System;
using System.Threading.Tasks;

namespace Nanny.Main
{
    public class Program
    {
        static Config _config = null;

        static KubeClient kube;
        static IScalableRule rule;
        static IQueueClient queueClient;

        static string containerName;
        static string containerImage;
        static string k8Namespace;
        static string k8Secret;
        static string queueName;
        static int containerLimit;
        static string jobCpuRequest;
        static string jobCpuLimit;
        static string jobMemRequest;
        static string jobMemLimit;
        static string jobConfigMapName;

        static async Task Main(string[] args)
        {
            _config = new Config(args,"dev");

            var connectionString = _config.GetRequired("QUEUE_CONNECTION_STRING");
            var kubeConfig = _config.GetNotRequired("K8S_CONIFG");
            queueName = _config.GetRequired("QUEUE_NAME");

            containerName = _config.GetRequired("JOB_CONTAINER_NAME");
            containerImage = _config.GetRequired("JOB_CONTAINER_IMAGE");
            k8Namespace = _config.GetRequired("K8S_NAMESPACE");
            k8Secret = _config.GetRequired("K8S_CR_SECRET");

            jobCpuRequest = _config.GetRequired("JOB_CPU_REQUEST");
            jobCpuLimit = _config.GetRequired("JOB_CPU_LIMIT");
            jobMemRequest = _config.GetRequired("JOB_MEM_REQUEST");
            jobMemLimit = _config.GetRequired("JOB_MEM_LIMIT");

            jobConfigMapName = _config.GetRequired("JOB_CONFIGMAP_NAME");

            try
            {
                containerLimit = int.Parse(_config.GetRequired("JOB_MAX_POD"));
            }
            catch
            {
                containerLimit = 5;
            }


            if (String.IsNullOrWhiteSpace(connectionString)) throw new ApplicationException("Connection String was not provided!!!");

            if (connectionString.IndexOf("AccountName") != -1) //Storage Account
                queueClient = new StorageQueueClient(connectionString);
            else
                queueClient = new ServiceBusQueueClient(connectionString); //Service Bus

            if (String.IsNullOrWhiteSpace(kubeConfig))
                kube = new KubeClient(); //In-Cluster Config
            else
                kube = new KubeClient(kubeConfig);

            rule = new IncrementRule(new TimeSpan(0, 1, 0)); // Scaling rule

            await CheckQueue(rule, queueName);
        }

        static async Task CheckQueue(IScalableRule rule, string queueName)
        {
            Console.WriteLine("Nanny.Checker is Running.");

            do
            {
                var messageCount = await GetMessageCount(queueName);
                Console.WriteLine($"There are {messageCount} messages in the queue {queueName}");
                
                var currentRunningJobs = await GetCurrentRunningJobs(k8Namespace, queueName);
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
                    await Task.Delay(5 * 60 * 1000);
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

            var configMap = await kube.GetConfigMapListAsync(k8Namespace, jobConfigMapName);

            if (configMap == null)
            {
                throw new ApplicationException($"Configuration Map '{jobConfigMapName}' for the nanny queue {queueName}");
            }
            
            var job_name = Guid.NewGuid().ToString();
            Console.WriteLine($"Job {job_name} has been created!");
            var job = await kube.CreateJobAsync(job_name, jobCount, 1, containerName, containerImage, k8Secret, queueName, configMap.Data, k8Namespace, jobCpuRequest, jobMemRequest, jobCpuLimit, jobMemLimit);
        }

        static async Task<int> GetCurrentRunningJobs(string _namespace = "default", string label = "")
        {
            return await kube.GetActivePodCountFromNamespaceAsync( _namespace, label);
        }

        #endregion
    }
}
