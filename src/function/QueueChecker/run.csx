#load "./util/Settings.csx"
#load "./util/Queue.csx"
#load "./util/Kube.csx"

public static async Task Run(string input, TraceWriter log)
{
    var messageCount = GetMessageCount();
    log.Info($"Messages: {Convert.ToString(messageCount)}");
  
    if (messageCount > 0)
       await CreateJob(1);
}

private static long GetMessageCount(){
    var connectionString = Settings.ServiceBusQueue.ConnectionString;
    var queueName = Settings.ServiceBusQueue.QueueName;

    return GetQueueMessageCount(connectionString, queueName);
}

private static async Task CreateJob(int pods)
{
    var containerName = Settings.Kubernetes.ContainerName;
    var containerImage = Settings.Kubernetes.ContainerImage;
    var server = Settings.Kubernetes.Server;
    var accessToken = Settings.Kubernetes.AccessToken;
    var jobName = $"{containerName}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";
    var parallelism = pods;
    var completions = pods;
    var imagePullSecret = Settings.Kubernetes.ImagePullSecret;

    await CreateKubeJob(server, accessToken, jobName, parallelism, completions, containerName, containerImage, imagePullSecret);
}