public static class Settings
{
    public static class ServiceBusQueue
    {
        public static string ConnectionString = "<YOUR SERVICE BUS CONNECTION STRING>";
        public static string QueueName = "<YOUR QUEUE NAME>";
    }

    public static class Kubernetes
    {
        public static string Server = "<YOUR KUBERNETES ROOT ENDPOINT>";
        public static string AccessToken = "<YOUR KUBERNETES STATIC TOKEN>";

        public static string ContainerName = "<YOUR CONTAINER NAME>";
        public static string ContainerImage = "<YOUR CONTAINER IMAGE>";
        public static string ImagePullSecret = "<YOUR PRIVATE REGISTRY SECRET>";
    }

    public static class Function
    {
        public static string SharedCertPath = "D:/home/site/wwwroot/<YOUR FUNCTION NAME>/cert";
    }
}