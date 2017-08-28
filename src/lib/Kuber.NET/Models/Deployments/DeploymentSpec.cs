namespace KuberNET.Models
{
    public class DeploymentSpec
    {
        public int replicas { get; set; }
        public DeploymentTemplate template { get; set; }
        public Strategy strategy { get; set; }
    }

}
