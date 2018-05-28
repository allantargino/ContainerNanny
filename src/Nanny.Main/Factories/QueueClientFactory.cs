using Nanny.Common.Interfaces;
using Nanny.Queue.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nanny.Main.Factories
{
    public static class QueueClientFactory
    {
        public static IQueueClient GetQueueClientFromConnectionString(string connectionString)
        {
            if (connectionString.IndexOf("AccountName") != -1) //Storage Account
               return new StorageQueueClient(connectionString);
            else
                return new ServiceBusQueueClient(connectionString); //Service Bus
        }
    }
}
