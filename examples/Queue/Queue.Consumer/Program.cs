using Microsoft.Azure.ServiceBus;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Queue.Consumer
{
    public class Program
    {
        static QueueClient _client;
        static SemaphoreSlim _semaphore;
        static DateTime _lastExecution;

        static Program()
        {
            _client = new QueueClient(Settings.ConnectionString, Settings.QueueName);
            _semaphore = new SemaphoreSlim(1, 1);
            _lastExecution = DateTime.Now;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Started!");

            RegisterOnMessageHandlerAndReceiveMessages();

            while (true)
            {
                _semaphore.Wait();
                if ((DateTime.Now - _lastExecution).Seconds > Settings.MaxIdleSeconds)
                    break;
                else
                {
                    Thread.Sleep(Settings.ThreadSleepTime);
                    _semaphore.Release();
                }
            }

            Console.WriteLine("Finished!");
        }

        static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _client.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            await _semaphore.WaitAsync();
            try
            {
                _lastExecution = DateTime.Now;
                Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
                Console.Out.Flush();
                await _client.CompleteAsync(message.SystemProperties.LockToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            return Task.CompletedTask;
        }
    }
}
