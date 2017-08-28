namespace KuberNET.Models
{
    public class Strategy
    {
        public string type { get; set; }
        public DeploymentRollingUpdate rollingUpdate { get; set; }
    }

}
