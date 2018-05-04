using System.Threading.Tasks;
using Nanny.Common.Interfaces;
using k8s.Models;

namespace Nanny.Kubernetes.Interfaces
{
    public interface IKubeScaling
    {
        //Deployments
        Task<V1Deployment> GetDeploymentAsync(string deployment, string _namespace);
        Task<V1Deployment> UpdateDeploymentAsync(V1Deployment deployment);

        //Jobs
        Task<V1Job> CreateJobAsync(string jobName, int parallelism, int completions, string containerName, string containerImage, string imagePullSecret, string _namespace);

        //Kluster
        Task<bool> isResourceAvailableAsync();
    }
}