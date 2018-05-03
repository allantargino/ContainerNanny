using Nanny.Common.Interfaces;
using System;
using System.Threading.Tasks;

namespace Nanny.StorageQueue
{
    public class StorageQueueClient : IQueueClient
    {
        public Task<long> GetMessageCountAsync(string queueName)
        {
            throw new NotImplementedException();
        }
    }
}
