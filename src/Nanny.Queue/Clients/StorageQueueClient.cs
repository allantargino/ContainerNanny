using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Nanny.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nanny.Queue.Clients
{
    public class StorageQueueClient : IQueueClient
    {
        CloudStorageAccount _storageAccount;
        CloudQueueClient _queueClient;
        CloudQueue _queue;

        public StorageQueueClient(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("connectionString is empty");

            try
            {
                _storageAccount = CloudStorageAccount.Parse(connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing the storage connection string {connectionString}: {ex.Message}");
            }

            _queueClient = _storageAccount.CreateCloudQueueClient();
        }

        private async Task<CloudQueue> GetQueueAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is empty!");
            if (_queueClient == null) throw new Exception("_queueClient is null!");

            var queue = _queueClient.GetQueueReference(name);

            var exists = await queue.ExistsAsync();
            if (!exists)
            {
                throw new Exception($"Error queue {name} doesn't exists!");
            }
            
            return queue;
        }

        public async Task<long> GetMessageCountAsync(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("queueName is empty");

            _queue = await GetQueueAsync(queueName);
            int? count = 0;
            try
            {
                await _queue.FetchAttributesAsync();
                count = _queue.ApproximateMessageCount;

                if (count > 0)
                {
                    List<CloudQueueMessage> messages = (List<CloudQueueMessage>)await _queue.PeekMessagesAsync(count.Value);

                    if (messages.Count < 32)
                        count = messages.Count;
                }
                Console.WriteLine($"The queue {queueName} has  {count} message(s)!");
            }
            catch (Exception ex) {
                throw new Exception($"Error getting message count: {ex.Message} ");
            }

            if (count != null)
                return long.Parse(count.ToString());

            return 0;
        }
    }
}
