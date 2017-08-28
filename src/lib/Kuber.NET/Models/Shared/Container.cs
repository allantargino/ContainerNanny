using System.Collections.Generic;

namespace KuberNET.Models
{
    public class Container
    {
        public string name { get; set; }
        public string image { get; set; }
        public List<Port> ports { get; set; }
        public Resources resources { get; set; }
        public LivenessProbe livenessProbe { get; set; }
        public ReadinessProbe readinessProbe { get; set; }
        public string terminationMessagePath { get; set; }
        public string terminationMessagePolicy { get; set; }
        public string imagePullPolicy { get; set; }
    }

}
