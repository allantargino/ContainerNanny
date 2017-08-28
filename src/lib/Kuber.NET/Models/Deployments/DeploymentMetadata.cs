using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuberNET.Models
{
    public class DeploymentMetadata
    {
        public string name { get; set; }
        public string selfLink { get; set; }
        public DeploymentLabels labels { get; set; }
    }
}
