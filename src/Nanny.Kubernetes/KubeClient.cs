using k8s;
using k8s.Models;
using Nanny.Common.Interfaces;
using Nanny.Kubernetes.Interfaces;
using System;
using System.Linq;
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

        public KubeClient()
        {
            _k8SClientConfig = KubernetesClientConfiguration.InClusterConfig();
            _k8client = new k8s.Kubernetes(_k8SClientConfig);
        }

        public KubeClient(FileInfo kubeconfig)
        {
            _k8SClientConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(kubeconfig);
            _k8client = new k8s.Kubernetes(_k8SClientConfig);
        }

        public KubeClient(string kubeconfigPath) : this(new FileInfo(kubeconfigPath)) { }

        public async Task<V1Deployment> GetDeploymentAsync(string deployment, string _namespace = "default")
        {
            if (string.IsNullOrEmpty(deployment)) throw new ArgumentException(nameof(deployment));

            return await _k8client.ReadNamespacedDeploymentAsync(deployment, _namespace, true);
        }

        public async Task<V1Deployment> UpdateDeploymentAsync(V1Deployment deployment)
        {
            if (deployment == null) throw new ArgumentNullException(nameof(deployment));

            var patch = new V1Patch(deployment);

            return await _k8client.PatchNamespacedDeploymentAsync(patch, deployment.Metadata.Name, deployment.Metadata.NamespaceProperty);
        }

        public async Task<V1Job> CreateJobAsync(string jobName, 
            int parallelism, 
            int completions, 
            string containerName, 
            string containerImage, 
            string imagePullSecret, 
            string label, 
            IDictionary<string, string> environmentVariables,
            string _namespace = "default",
            string cpuRequest="250m",
            string memRequest="50Mi",
            string cpuLimit="500m",
            string memLimit="100Mi")
        {
            if (string.IsNullOrEmpty(jobName)) throw new ArgumentNullException(nameof(jobName));
            if (string.IsNullOrEmpty(containerName)) throw new ArgumentNullException(nameof(containerName));
            if (string.IsNullOrEmpty(containerImage)) throw new ArgumentNullException(nameof(containerImage));
            if (string.IsNullOrEmpty(imagePullSecret)) throw new ArgumentNullException(nameof(imagePullSecret));
            if (parallelism < 1) throw new ArgumentOutOfRangeException(nameof(parallelism));
            if (completions < 1) throw new ArgumentOutOfRangeException(nameof(completions));
            if (environmentVariables == null || environmentVariables.Count == 0) throw new ApplicationException("No Environment variable provided!");

            var labels = new Dictionary<string, string>();
            labels.Add("nanny", label);

            var job = new V1Job();
            job.Metadata = new V1ObjectMeta()
            {
                Name = jobName
            };

            var podLimits = new Dictionary<string, ResourceQuantity>();
            podLimits.Add("cpu", new ResourceQuantity(cpuLimit));
            podLimits.Add("memory", new ResourceQuantity(memLimit));

            var podRequest = new Dictionary<string, ResourceQuantity>();
            podRequest.Add("cpu", new ResourceQuantity(cpuRequest));
            podRequest.Add("memory", new ResourceQuantity(memRequest));

            List<V1EnvVar> envVars = environmentVariables.Select(e => new V1EnvVar(e.Key, e.Value)).ToList<V1EnvVar>();

            job.Spec = new V1JobSpec()
            {
                Parallelism = parallelism,
                Completions = completions,
                Template = new V1PodTemplateSpec()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Labels = labels
                    },
                    Spec = new V1PodSpec()
                    {
                        Containers = new List<V1Container>() {
                            new V1Container(){
                                Name = containerName,
                                Image = containerImage,
                                ImagePullPolicy = "Always",
                                Resources = new V1ResourceRequirements()
                                {
                                    Limits = podLimits,
                                    Requests = podRequest
                                },
                                Env = envVars
                            }
                        },
                        RestartPolicy = "Never",
                        ImagePullSecrets = new List<V1LocalObjectReference>()
                        {
                            new V1LocalObjectReference(){
                                Name = imagePullSecret
                            }
                        }
                    }
                }
            };

            return await _k8client.CreateNamespacedJobAsync(job, _namespace);
        }

        public async Task<int> GetActivePodCountFromNamespaceAsync(string _namespace = "default", string label = "")
        {
            var count = 0;
            if (!string.IsNullOrWhiteSpace(label))
            {
                count = (await _k8client.ListNamespacedPodAsync(_namespace, fieldSelector: "status.phase=Pending", labelSelector: $"nanny={label}")).Items.Count;
                count += (await _k8client.ListNamespacedPodAsync(_namespace, fieldSelector: "status.phase=Running", labelSelector: $"nanny={label}")).Items.Count;
            }
            else
            {
                count = (await _k8client.ListNamespacedPodAsync(_namespace, fieldSelector: "status.phase=Pending")).Items.Count;
                count += (await _k8client.ListNamespacedPodAsync(_namespace, fieldSelector: "status.phase=Running")).Items.Count;
            }

            return count;
        }

        public async Task<bool> isResourceAvailableAsync()
        {
            //TODO: Call the api to check if there is resources (CPU Request and Memory) available to the cluster
            return await Task.FromResult(true);
        }

        public async Task<V1ConfigMap> GetConfigMapListAsync(string _namespace, string configMapListName)
        {
            var configMapList = await _k8client.ListNamespacedConfigMapAsync(_namespace);

            if (configMapList.Items.Count > 0)
            {
                var configMapResult = configMapList.Items.Where(c => c.Metadata.Name.Equals(configMapListName)).First();
                return configMapResult;
            }

            return null;
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
