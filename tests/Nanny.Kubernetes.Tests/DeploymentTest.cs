using k8s;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;

namespace Nanny.Kubernetes.Tests
{
    [TestClass]
    public class DeploymentTest
    {
        [TestMethod]
        public void ConnectOnClusterNative()
        {
            //TODO: Restore this tests
            //var kubeconfig = new FileInfo("./config");
            //var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(kubeconfig);
            //IKubernetes client = new k8s.Kubernetes(config);

            //var list = client.ListNamespacedPod("kube-system");
            //Assert.AreEqual(list.Items.Count > 0, true);

            //foreach (var item in list.Items)
            //    Trace.WriteLine(item.Metadata.Name);
        }

        [TestMethod]
        public void ConnectOnClusterFromOurClient()
        {
        }

        [TestMethod]
        public void GetDeployment()
        {
        }
    }
}
