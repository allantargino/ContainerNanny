namespace KuberNET.Models
{
    public class ReadinessProbe
    {
        public HttpGet httpGet { get; set; }
        public int timeoutSeconds { get; set; }
        public int periodSeconds { get; set; }
        public int successThreshold { get; set; }
        public int failureThreshold { get; set; }
    }

}
