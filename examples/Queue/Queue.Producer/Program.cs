using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Queue.Producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Started!");

            var cts = new CancellationTokenSource();
            var token = cts.Token;
            Console.WriteLine("Press any key to cancel...");

            Task.Run(() =>
            {
                var random = new Random();
                var client = QueueClient.CreateFromConnectionString(Settings.ConnectionString, Settings.QueueName);
                while (!cts.IsCancellationRequested)
                {
                    var text = random.Next().ToString();
                    var msg = new BrokeredMessage(text);
                    client.Send(msg);

                    Console.WriteLine($"Message {msg.MessageId}, Text: {msg.GetBody<string>()}");
                    Thread.Sleep(Settings.MessageDelay);
                }
            });

            Console.ReadLine();
            cts.Cancel();
            Console.WriteLine("Finished!");
        }
    }
}
