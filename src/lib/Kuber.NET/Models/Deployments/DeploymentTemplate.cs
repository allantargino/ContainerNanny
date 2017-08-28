namespace KuberNET.Models
{
    public class DeploymentTemplate
    {
        public DeploymentMetadata metadata { get; set; }
        public DeploymentTemplateSpec spec { get; set; }
    }

}
