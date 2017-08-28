#load "./util/Settings.csx"
#load "./util/Queue.csx"
#load "./util/Kube.csx"

private static string containerName = "<YOUR CONTAINER NAME>";
private static string containerImage = "<YOUR CONTAINER IMAGE>";
private static int pods = 1;

public static async Task Run(string input, TraceWriter log)
{
    var messageCount = GetMessageCount();
    log.Info($"Messages: {Convert.ToString(messageCount)}");
  
    if (messageCount > 0)
       await ScaleKubJob(containerName, containerImage, pods);
}

private static long GetMessageCount(){
    var connectionString = Settings.ServiceBusQueue.ConnectionString;
    var queueName = Settings.ServiceBusQueue.QueueName;

    return GetQueueMessageCount(connectionString, queueName);
}

private static async Task ScaleKubJob(string containerName, string containerImage, int pods)
{
    var server = Settings.Kubernetes.Server;
    var accessToken = Settings.Kubernetes.AccessToken;
    var jobName = $"{containerName}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";
    var parallelism = pods;
    var completions = pods;
    var imagePullSecret = Settings.Kubernetes.ImagePullSecret;

    await CreateKubeJob(server, accessToken, jobName, parallelism, completions, containerName, containerImage, imagePullSecret);
}