#load "./util/Settings.csx"
using Microsoft.Azure.ServiceBus;
using System.Security.Cryptography.X509Certificates;


private static string containerName = "queueconsumer";
private static string containerImage = "visouza.azurecr.io/app/queueconsumer";
private static int pods = 1;
private const string SHARED_PATH = "D:/home/site/wwwroot/QueueChecker/cert";

private static QueueClient _queueClient;

public static async Task Run(string input, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
    CheckQueue().Wait();
}

private static async Task CheckQueue()
{
    _queueClient = new QueueClient(Settings.ServiceBusQueue.ConnectionString, Settings.ServiceBusQueue.QueueName);

    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
    {
        // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
        // Set it according to how many messages the application wants to process in parallel.
        MaxConcurrentCalls = 1,

        // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
        // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
        AutoComplete = false

    };

        // Register the function that will process messages
    _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

    await _queueClient.CloseAsync();
}

static async Task ProcessMessagesAsync(Message message, CancellationToken token)
{
    // Process the message
    Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

    await CreateKubeJob();

}

static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
{
    Console.WriteLine($"Message handler encountered an exception {arg.Exception}.");
    var context = arg.ExceptionReceivedContext;
    Console.WriteLine("Exception context for troubleshooting:");
    Console.WriteLine($"- Endpoint: {context.Endpoint}");
    Console.WriteLine($"- Entity Path: {context.EntityPath}");
    Console.WriteLine($"- Executing Action: {context.Action}");
    return Task.CompletedTask;
}

static X509Certificate GetCert(string filename)
{
    return new X509Certificate2($"{SHARED_PATH}/{filename}");
}


static async Task CreateKubeJob()
{
    var cert = GetCert(@"ca.crt");
    using (var client = new KuberClient(Settings.Kubernetes.Server, Settings.Kubernetes.AccessToken, cert))
    {
        var job = await client.CreateJobAsync(Guid.NewGuid().ToString(), 1, 1, "queueconsumer", "visouza.azurecr.io/app/queueconsumer", "qOyVY5WhDfuvTUT+pcF9z8lIX1nDNDL0");
    }
}