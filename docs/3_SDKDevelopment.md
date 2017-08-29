# SDK Development

We would like to perform 2 operation on Kubernetes:
1. Scale deployment replicas
    * This allows full control for the Function handles the containers lifecycle.
2. Create jobs
    * This allows Function handles the containers creation and the containers should terminated themselves.

A great [C# Kubernetes client library](https://github.com/kubernetes-client/csharp) is being developed by [Kubernetes Clients](https://github.com/kubernetes-client) organization in Github. But since we need to deploy it on Azure Functions and they don't have a Nuget package yet, we decided to develop these 2 simple operations.

## Development Approach

Using Static Access token from previously section, we make a REST call to the following URLs:

[Get Deployment](https://kubernetes.io/docs/api-reference/v1.6/#read-27):
>GET /apis/apps/v1beta1/namespaces/{namespace}/deployments/{name}

[Get Job](https://kubernetes.io/docs/api-reference/v1.6/#read-44)
>GET /apis/batch/v1/namespaces/{namespace}/jobs/{name}

Then we used [json2csharp](http://json2csharp.com) to generate the models from the calls' answers above. The developed methods signature is the following:

```cs
public interface IKubeScaling
{
    //Deployments
    Task<Deployment> GetDeploymentAsync(string deployment, string _namespace);
    Task<Deployment> UpdateDeploymentAsync(Deployment deployment);

    //Jobs
    Task<Job> CreateJobAsync(string jobName, int parallelism, int completions, string containerName, string containerImage, string imagePullSecret, string _namespace);
}
```

## Authentication

As mentioned on the previously section, Kubernetes API supports many authentication methods. We developed the *KuberClient* to be able to work with both Client Certificate and Static Tokens, as the following:

```cs
public KuberClient(string serverUrl, string accessToken, X509Certificate caCertificate)
```
or
``` cs
public KuberClient(string baseUrl, X509Certificate clientCertificate, X509Certificate caCertificate)
```

Also, as related on Certificate Setup section, Kubernetes setup generated a CA root. In order to make HTTPS calls without errors during TLS handshake, we should programmatically add the Certificate Authority to the Certificate verification Chain.

We use a WebRequestHandler object to configure these settings. First, we create it and pass the verification callback on it:
```cs
var requestHandler = new WebRequestHandler
{
    ServerCertificateValidationCallback = ServerCertificateValidationCallback
};

[...]

//The following validation function was based on https://github.com/kubernetes-client/csharp repository implementation.
private bool ServerCertificateValidationCallback(object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
{
    if (sslPolicyErrors.Equals(SslPolicyErrors.None))
        return true; //No errors.

    if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) > 0)
    {
        x509Chain.ChainPolicy.ExtraStore.Add(_caCertificate); //CA Kube Cert.
        x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority; //Allow not Store added CAs.
        return x509Chain.Build((X509Certificate2)x509Certificate); //Builds and checks the certificate chain is guaranteed by some CA. 
    }

    return false; //Some error.
}
```

After that, we just use this WebRequestHandler instance and pass to HttpClient object:

```cs
_http = new HttpClient(requestHandler);
_http.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
```

Also, we should set the SecurityProtocol to use TLS 1.2, since in some versions of .NET Framework it is not the default version.
> Azure Functions, by now, uses .NET 4.5, which by default is SecurityProtocolType.Tls and SecurityProtocolType Ssl3. If you removed the last line from the code above, the TLS handshake will fail with the message: "The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."

After the SDK was finished, we built it on Release Mode and save the .dll file to use in Azure Function - process describe on next section, Function Development.

## References
* [X509Chain.Build Method (X509Certificate2)](https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.x509chain.build(v=vs.110).aspx)
* [X509Chain.ChainPolicy Property](https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.x509chain.chainpolicy(v=vs.110).aspx)
* [WebRequestHandler.ServerCertificateValidationCallback Property](https://msdn.microsoft.com/en-us/library/system.net.http.webrequesthandler.servercertificatevalidationcallback(v=vs.110).aspx)
* [TLS 1.2 and .NET Support: How to Avoid Connection Errors](http://blogs.perficient.com/microsoft/2016/04/tsl-1-2-and-net-support/)
* [Kubernetes API reference (1.6)](https://kubernetes.io/docs/api-reference/v1.6/)