using KuberNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuberNET.Interfaces
{
    public interface IKubeScaling
    {
        //Deployments
        Task<Deployment> GetDeploymentAsync(string deployment, string _namespace);
        Task<Deployment> UpdateDeploymentAsync(Deployment deployment);

        //Jobs
        Task<Job> CreateJobAsync(string jobName, int parallelism, int completions, string containerName, string containerImage, string imagePullSecret, string _namespace);
    }
}