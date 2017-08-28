using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuberNET.Models
{
    public class JobSpec
    {
        public int parallelism { get; set; }
        public int completions { get; set; }
        public JobTemplate template { get; set; }
    }

}
