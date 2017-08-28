using KuberNET;
using Microsoft.ServiceBus;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Queue.Checker
{
    public class Program
    {
        static void Main(string[] args)
        {
            CheckQueue().Wait();
            Console.ReadLine();
        }

        private static async Task CheckQueue()
        {
            var manager = NamespaceManager.CreateFromConnectionString(Settings.ServiceBusQueue.ConnectionString);
            long messageCount = manager.GetQueue(Settings.ServiceBusQueue.QueueName).MessageCount;
            var details = manager.GetQueue(Settings.ServiceBusQueue.QueueName).MessageCountDetails;

            if (messageCount > 0)
            {
                await CreateKubeJob(1);
                Console.WriteLine("The job was created!");
            }
            else
            {
                Console.WriteLine($"Queue {Settings.ServiceBusQueue.QueueName} does not have any items.");
            }
        }

        private static X509Certificate GetCert(string filename)
        {
            return new X509Certificate2(filename);
        }

        static async Task ScaleKubeDeployment(long messageCount)
        {
            var cert = GetCert(Settings.Kubernetes.CACertificatePath);
            using (var client = new KuberClient(Settings.Kubernetes.Server, Settings.Kubernetes.AccessToken, cert))
            {
                var deployment = await client.GetDeploymentAsync(Settings.Kubernetes.Deployment.Name);
                deployment.spec.replicas+= Settings.Kubernetes.Deployment.Replicas;
                await client.UpdateDeploymentAsync(deployment);
            }
        }

        static async Task CreateKubeJob(int pods)
        {
            var cert = GetCert(Settings.Kubernetes.CACertificatePath);

            var containerName = Settings.Kubernetes.Job.ContainerName;
            var containerImage = Settings.Kubernetes.Job.ContainerImage;
            var server = Settings.Kubernetes.Server;
            var accessToken = Settings.Kubernetes.AccessToken;
            var jobName = $"{containerName}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";
            var parallelism = pods;
            var completions = pods;
            var imagePullSecret = Settings.Kubernetes.Job.ImagePullSecret;

            using (var client = new KuberClient(Settings.Kubernetes.Server, Settings.Kubernetes.AccessToken, cert))
            {
                var job  = await client.CreateJobAsync(jobName, parallelism, completions, containerName, containerImage, imagePullSecret);
            }
        }
    }
}
