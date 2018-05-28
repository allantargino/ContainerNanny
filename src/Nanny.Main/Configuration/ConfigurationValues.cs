using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Main.Configuration
{
    public class ConfigurationValues
    {
        public ConfigurationQueue Queue { get; private set; }
        public ConfigurationKubernetes Kubernetes { get; private set; }
        public ConfigurationNanny Nanny { get; private set; }

        public static ConfigurationValues GetFromConfigurationManager(ConfigurationManager configurationManager)
        {
            return new ConfigurationValues()
            {
                Queue = new ConfigurationQueue()
                {
                    ConnectionString = configurationManager.GetRequired("QUEUE_CONNECTION_STRING"),
                    QueueName = configurationManager.GetRequired("QUEUE_NAME"),
                },
                Kubernetes = new ConfigurationKubernetes()
                {
                    KubeConfig = configurationManager.GetNotRequired("K8S_CONFIG", string.Empty),
                    ContainerName = configurationManager.GetRequired("JOB_CONTAINER_NAME"),
                    ContainerImage = configurationManager.GetRequired("JOB_CONTAINER_IMAGE"),
                    K8Namespace = configurationManager.GetNotRequired("K8S_NAMESPACE", "default"),
                    K8Secret = configurationManager.GetRequired("K8S_CR_SECRET"),
                    JobCpuRequest = configurationManager.GetRequired("JOB_CPU_REQUEST"),
                    JobCpuLimit = configurationManager.GetRequired("JOB_CPU_LIMIT"),
                    JobMemRequest = configurationManager.GetRequired("JOB_MEM_REQUEST"),
                    JobMemLimit = configurationManager.GetRequired("JOB_MEM_LIMIT"),
                    JobConfigMapName = configurationManager.GetRequired("JOB_CONFIGMAP_NAME"),
                    ContainerLimit = configurationManager.GetNotRequired("JOB_MAX_POD", 5)
                },
                Nanny = new ConfigurationNanny()
                {
                }
            };
        }
    }
}
