# Function Development

The code discussed here can be found inside *src/Function* path. You can deploy the whole folder to your App Service by FTP. The following steps show the development process we faced.

## Development Steps:

### 1. Create a bin folder inside your Function and deploy the Kuber.NET.dll binary.

We basically used the binary generated from previously step and uploaded it to Azure Function FTP path, **under /bin folder.**

### 2. Upload your CA Kubernetes certificate file.

It was created a folder with /cert name and uploaded through FTP the CA file generated in *Certificate Setup* section.

### 3. Add the references in a project.json file.

We created and upload the following dependencies json file to the root path from Function:

```json
{
  "frameworks": {
    "net46":{
      "dependencies": {
        "WindowsAzure.ServiceBus": "4.1.3"
      }
    }
   }
}
```

### 4. Develop code snippets for Kubernetes and Service Bus Queue.

### - util\Kube.csx: 

* We load the custom assembly
* Access and get certificate from Function location
* Call Kube API

```cs
#r "D:\home\site\wwwroot\QueueChecker\bin\Kuber.NET.dll"

using KuberNET;
using System.Security.Cryptography.X509Certificates;

private const string SHARED_PATH = "D:/home/site/wwwroot/QueueChecker/cert";

static async Task CreateKubeJob(string server, string accessToken, string jobName, int parallelism, int completions, string containerName, string containerImage, string imagePullSecret)
{
    var cert = GetCertificate("ca.crt");
    using (var client = new KuberClient(server, accessToken, cert))
    {
        var job = await client.CreateJobAsync(jobName, parallelism, completions, containerName, containerImage, imagePullSecret);
    }
}

private static X509Certificate GetCertificate(string file)
{
    var data = File.ReadAllBytes($"{SHARED_PATH}/{file}");
    return new X509Certificate2(data);
}
```

### - util\Queue.csx: 

* Just use Service Bus SDK to get the queue length.

> We used Service Bus SDK from .NET Full. Great examples for .NET Core can be found [here](https://github.com/Azure/azure-service-bus/tree/master/samples/DotNet/Microsoft.Azure.ServiceBus)

```cs
using Microsoft.ServiceBus;

private static long GetQueueMessageCount(string connectionString, string queueName)
{
    var manager = NamespaceManager.CreateFromConnectionString(connectionString);
    long messageCount = manager.GetQueue(queueName).MessageCount;

    return messageCount;
}
```

### 5. Fill the Settings file with your configuration.
It was also created a settings file on /util/Settings.csx, with the following:

* ServiceBusQueue.ConnectionString
* ServiceBusQueue.QueueName
* Kubernetes.Server
* Kubernetes.AccessToken
* Kubernetes.ImagePullSecret

> You just need to fill Kubernetes.ImagePullSecret in case you use a private container images repository, like Azure Container Registry.

> If you want to use Client Certificates authentication instead Access Tokens one with Azure Functions, upload your .pfx files through App Service panel, and instantiate KuberClient with the certificate. A good thread on the subject can be found [here](https://stackoverflow.com/questions/40240195/runtime-error-loading-certificate-in-azure-functions/40247512).
> Specially with Kubernetes, use openssl to generate your .pfx files using the public and private keys in .kube/config file.

### 6. Implement you business logic inside **run.csx** file

* We loaded the dependencies from the developed code snippets.
* Fill the container information on the constants
    > We didn't put on Settings file in order to be easier the container info changing, but you can delegate all settings on that file.
* The function will run every minute. If there is any message, create a job to help process the queue.

```cs
#load "./util/Settings.csx"
#load "./util/Queue.csx"
#load "./util/Kube.csx"

private static string containerName = "queueconsumer";
private static string containerImage = "server.azurecr.io/app/queueconsumer";
private static int pods = 1;

public static async Task Run(string input, TraceWriter log)
{
    var messageCount = GetMessageCount();
    log.Info($"Messages: {Convert.ToString(messageCount)}");
  
    if (messageCount > 0)
       await ScaleKubJob(containerName, containerImage, pods);
}
```

## References
* [Azure Functions - C# script developer reference](https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference-csharp)
* [Azure Functions - Performance considerations](https://docs.microsoft.com/en-us/azure/azure-functions/functions-best-practices)
* [Service Bus Queue - .NET Core Samples](https://github.com/Azure/azure-service-bus/tree/master/samples/DotNet/Microsoft.Azure.ServiceBus)
