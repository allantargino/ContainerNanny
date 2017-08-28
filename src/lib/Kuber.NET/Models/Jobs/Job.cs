using KuberNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuberNET.Models
{
    public class Job
    {
        public JobMetadata metadata { get; set; }
        public JobSpec spec { get; set; }
    }
}
