using System.Threading.Tasks;

namespace Nanny.Common.Interfaces
{
    public interface IQueueClient
    {
        Task<long> GetMessageCountAsync(string queueName);
    }
}