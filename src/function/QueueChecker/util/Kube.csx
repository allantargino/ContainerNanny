#r "D:\home\site\wwwroot\QueueChecker\bin\Kuber.NET.dll"

using KuberNET;
using System.Security.Cryptography.X509Certificates;

private const string SHARED_PATH = "D:/home/site/wwwroot/QueueChecker/cert";

static async Task ScaleKubeDeployment(string server, string accessToken, string deploymentName, int replicasIncreaseCount)
{
    var cert = GetCertificate("ca.crt");
    using (var client = new KuberClient(server, accessToken, cert))
    {
        var deployment = await client.GetDeploymentAsync(deploymentName);
        deployment.spec.replicas+=replicasIncreaseCount;
        await client.UpdateDeploymentAsync(deployment);
    }
}

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