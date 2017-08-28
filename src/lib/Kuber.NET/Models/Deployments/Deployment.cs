using KuberNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuberNET.Models
{
    public class Deployment
    {
        public DeploymentSpec spec { get; set; }
        public DeploymentMetadata metadata { get; set; }
    }
}
