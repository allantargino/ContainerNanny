using KuberNET.Interfaces;
using KuberNET.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KuberNET
{
    public class KuberClient : IDisposable, IKubeScaling
    {
        private string _baseUrl;
        private string _accessToken;
        private HttpClient _http;
        private X509Certificate _clientCertificate;
        private X509Certificate _caCertificate;

        public KuberClient(string serverUrl, string accessToken, X509Certificate caCertificate)
        {
            if (string.IsNullOrEmpty(serverUrl)) throw new ArgumentNullException(nameof(serverUrl));
            if (string.IsNullOrEmpty(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (caCertificate == null) throw new ArgumentNullException(nameof(caCertificate));

            var requestHandler = new WebRequestHandler
            {
                ServerCertificateValidationCallback = ServerCertificateValidationCallback
            };

            _http = new HttpClient(requestHandler);
            _http.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            _baseUrl = serverUrl;
            _accessToken = accessToken;
            _caCertificate = caCertificate;
        }

        public KuberClient(string baseUrl, X509Certificate clientCertificate, X509Certificate caCertificate)
        {
            if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));
            if (clientCertificate == null) throw new ArgumentNullException(nameof(clientCertificate));
            if (caCertificate == null) throw new ArgumentNullException(nameof(caCertificate));

            var requestHandler = new WebRequestHandler();
            requestHandler.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
            requestHandler.ClientCertificates.Add(clientCertificate);

            _http = new HttpClient(requestHandler);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            _baseUrl = baseUrl;
            _clientCertificate = clientCertificate;
            _caCertificate = caCertificate;
        }

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


        public async Task<Deployment> GetDeploymentAsync(string deployment, string _namespace = "default")
        {
            if (string.IsNullOrEmpty(deployment)) throw new ArgumentException(nameof(deployment));

            var url = $"{_baseUrl}/apis/extensions/v1beta1/namespaces/{_namespace}/deployments/{deployment}";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Deployment>(body);
        }

        public async Task<Deployment> UpdateDeploymentAsync(Deployment deployment)
        {
            if (deployment == null) throw new ArgumentNullException(nameof(deployment));

            var url = $"{_baseUrl}{deployment.metadata.selfLink}";

            var requestObject = new StringContent(JsonConvert.SerializeObject(deployment), Encoding.UTF8, "application/json");

            var response = await _http.PutAsync(url, requestObject);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Deployment>(body);
        }

        public async Task<Job> CreateJobAsync(string jobName, int parallelism, int completions, string containerName, string containerImage, string imagePullSecret, string _namespace = "default")
        {
            if (string.IsNullOrEmpty(jobName)) throw new ArgumentNullException(nameof(jobName));
            if (string.IsNullOrEmpty(containerName)) throw new ArgumentNullException(nameof(containerName));
            if (string.IsNullOrEmpty(containerImage)) throw new ArgumentNullException(nameof(containerImage));
            if (string.IsNullOrEmpty(imagePullSecret)) throw new ArgumentNullException(nameof(imagePullSecret));
            if (parallelism < 1) throw new ArgumentOutOfRangeException(nameof(parallelism));
            if (completions < 1) throw new ArgumentOutOfRangeException(nameof(completions));

            var job = new Job()
            {
                metadata = new JobMetadata()
                {
                    name = jobName
                },
                spec = new JobSpec()
                {
                    parallelism = parallelism,
                    completions = completions,
                    template = new JobTemplate()
                    {
                        spec = new JobTemplateSpec()
                        {
                            containers = new List<Container>() {
                                new Container()
                                {
                                    name = containerName,
                                    image = containerImage
                                }
                            },
                            restartPolicy = "Never",
                            imagePullSecrets  = new List<ImagePullSecret>()
                            {
                                new ImagePullSecret()
                                {
                                    name = imagePullSecret
                                }
                            }
                        }
                    }
                }
            };

            var url = $"{_baseUrl}/apis/batch/v1/namespaces/{_namespace}/jobs";

            var content = JsonConvert.SerializeObject(job);
            var response = await _http.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Job>(body);
        }

        #region IDisposable Support

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _http.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
