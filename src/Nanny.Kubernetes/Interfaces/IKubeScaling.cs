using System.Threading.Tasks;
using Nanny.Common.Interfaces;
using k8s.Models;
using System.Collections.Generic;

namespace Nanny.Kubernetes.Interfaces
{
    public interface IKubeScaling
    {
        //Deployments
        Task<V1Deployment> GetDeploymentAsync(string deployment, string _namespace);
        Task<V1Deployment> UpdateDeploymentAsync(V1Deployment deployment);

        //Jobs
        Task<V1Job> CreateJobAsync(string jobName, int parallelism, int completions, string containerName, string containerImage, string imagePullSecret, string label, IDictionary<string, string> environmentVariables, string _namespace, string cpuRequest, string memRequest, string cpuLimit, string memLimit);

        //Config Map
        Task<V1ConfigMap> GetConfigMapListAsync(string _namespace, string configMapListName);

        //Kluster
        Task<bool> IsResourceAvailableAsync();
    }
}