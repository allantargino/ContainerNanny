using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Main.Configuration
{
    public class ConfigurationKubernetes
    {
        public string ContainerName { get; set; }
        public string ContainerImage { get; set; }
        public string K8Namespace { get; set; }
        public string K8Secret { get; set; }
        public int ContainerLimit { get; set; }
        public string JobCpuRequest { get; set; }
        public string JobCpuLimit { get; set; }
        public string JobMemRequest { get; set; }
        public string JobMemLimit { get; set; }
        public string JobConfigMapName { get; set; }
        public string KubeConfig { get; set; }
    }
}
