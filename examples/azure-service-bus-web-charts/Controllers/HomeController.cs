using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nanny.Kubernetes;
using Nanny.ServiceBus;
using Queue.Web.Models;

namespace Queue.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration settings;

        public HomeController(IConfiguration configuration)
        {
            settings = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<long> MessageCount()
        {
            return await GetMessageCount();
        }

        private async Task<long> GetMessageCount()
        {
            return await new
                ServiceBusQueueClient(settings["ServiceBusConnectionString"])
                .GetMessageCountAsync(settings["ServiceBusQueueName"]);
        }

        public async Task<int> PodCount()
        {
            return await GetPodCount();
        }

        private async Task<int> GetPodCount()
        {
            return await new
                KubeClient(settings["KubernetesConfig"])
                .GetActivePodCountFromNamespaceAsync("default");
        }
    }
}
