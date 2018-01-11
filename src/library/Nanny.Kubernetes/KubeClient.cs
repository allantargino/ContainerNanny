using k8s;
using k8s.Models;
using Nanny.Common.Interfaces;
using Nanny.Kubernetes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Nanny.Kubernetes
{
    public class KubeClient : IDisposable, IKubeScaling, IOrchestratorClient
    {
        private KubernetesClientConfiguration _k8SClientConfig;
        private IKubernetes _k8client;

        public KubeClient(FileInfo kubeconfig)
        {
            _k8SClientConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(kubeconfig);
            _k8client = new k8s.Kubernetes(_k8SClientConfig);
        }

        public KubeClient(string kubeconfigPath) : this(new FileInfo(kubeconfigPath)) { }

        public async Task<Apiappsv1beta1Deployment> GetDeploymentAsync(string deployment, string _namespace = "default")
        {
            if (string.IsNullOrEmpty(deployment)) throw new ArgumentException(nameof(deployment));

            return await _k8client.ReadNamespacedDeploymentAsync(deployment, _namespace, true);

        }

        public async Task<Apiappsv1beta1Deployment> UpdateDeploymentAsync(Apiappsv1beta1Deployment deployment)
        {
            if (deployment == null) throw new ArgumentNullException(nameof(deployment));

            return await _k8client.PatchNamespacedDeploymentAsync(deployment, deployment.Metadata.Name, deployment.Metadata.NamespaceProperty);

        }

        public async Task<Apibatchv1Job> CreateJobAsync(string jobName, int parallelism, int completions, string containerName, string containerImage, string imagePullSecret, string _namespace = "default")
        {
            if (string.IsNullOrEmpty(jobName)) throw new ArgumentNullException(nameof(jobName));
            if (string.IsNullOrEmpty(containerName)) throw new ArgumentNullException(nameof(containerName));
            if (string.IsNullOrEmpty(containerImage)) throw new ArgumentNullException(nameof(containerImage));
            if (string.IsNullOrEmpty(imagePullSecret)) throw new ArgumentNullException(nameof(imagePullSecret));
            if (parallelism < 1) throw new ArgumentOutOfRangeException(nameof(parallelism));
            if (completions < 1) throw new ArgumentOutOfRangeException(nameof(completions));


            var job = new Apibatchv1Job();
            job.Metadata = new V1ObjectMeta()
            {
                Name = jobName
            };

            job.Spec = new Apibatchv1JobSpec() {
                Parallelism = parallelism,
                Completions = completions,
                Template = new Corev1PodTemplateSpec()
                {
                    Spec = new Corev1PodSpec()
                    {
                        Containers = new List<Corev1Container>() {
                        new Corev1Container(){
                            Name = containerName,
                            Image = containerImage
                        }
                    },
                        RestartPolicy = "Never",
                        ImagePullSecrets = new List<Corev1LocalObjectReference>()
                    {
                        new Corev1LocalObjectReference(){
                            Name = imagePullSecret
                        }
                    }

                    }   
                }
            };

            return await _k8client.CreateNamespacedJobAsync(job, _namespace);
        }

        public async Task<int> GetActivePodCountFromNamespaceAsync(string _namespace = "default")
        {
            return (await _k8client.ListNamespacedPodAsync(_namespace, fieldSelector: "status.phase=Running")).Items.Count;
        }

        #region IDisposable Support

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _k8client != null)
                {
                    _k8client.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
