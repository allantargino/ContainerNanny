using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Main.Configuration
{
    public class ConfigurationValues
    {
        public ConfigurationQueue Queue { get; set; }
        public ConfigurationKubernetes Kubernetes { get; set; }
        public ConfigurationNanny Nanny { get; set; }
    }
}
