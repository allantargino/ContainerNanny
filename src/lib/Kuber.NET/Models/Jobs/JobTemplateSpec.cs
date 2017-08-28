using System.Collections.Generic;

namespace KuberNET.Models
{
    public class JobTemplateSpec
    {
        public List<Container> containers { get; set; }
        public string restartPolicy { get; set; }
        public int terminationGracePeriodSeconds { get; set; }
        public string dnsPolicy { get; set; }
        public List<ImagePullSecret> imagePullSecrets { get; set; }
        public string schedulerName { get; set; }
    }

}
