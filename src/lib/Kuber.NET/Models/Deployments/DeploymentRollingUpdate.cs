namespace KuberNET.Models
{
    public class DeploymentRollingUpdate
    {
        public int maxUnavailable { get; set; }
        public int maxSurge { get; set; }
    }

}
