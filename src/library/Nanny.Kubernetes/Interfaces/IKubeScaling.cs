using System.Threading.Tasks;
using Nanny.Common.Interfaces;
using k8s.Models;

namespace Nanny.Kubernetes.Interfaces
{
    public interface IKubeScaling
    {
        //Deployments
        Task<Apiappsv1beta1Deployment> GetDeploymentAsync(string deployment, string _namespace);
        Task<Apiappsv1beta1Deployment> UpdateDeploymentAsync(Apiappsv1beta1Deployment deployment);

        //Jobs
        Task<Apibatchv1Job> CreateJobAsync(string jobName, int parallelism, int completions, string containerName, string containerImage, string imagePullSecret, string _namespace);
    }
}