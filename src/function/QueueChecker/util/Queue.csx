using Microsoft.ServiceBus;

private static long GetQueueMessageCount(string connectionString, string queueName)
{
    var manager = NamespaceManager.CreateFromConnectionString(connectionString);
    long messageCount = manager.GetQueue(queueName).MessageCount;

    return messageCount;
}