using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Producer
{
    class Program
    {
        static IMessageSender messageSender;
        const string SessionPrefix = "session-prefix";

        static JsonConfiguration settings;


        static void Main(string[] args)
        {
            settings = JsonConfiguration.Build("./settings.json");

            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            const int numberOfSessions = 1;
            const int numberOfMessagesPerSession = 300;


            while (true)
            {
                messageSender = new MessageSender(settings["ServiceBusConnectionString"], settings["ServiceBusQueueName"]);
                await SendSessionMessagesAsync(numberOfSessions, numberOfMessagesPerSession);
                await messageSender.CloseAsync();
                await Task.Delay(10 * 1000);
            }

            //// Send messages with sessionId set

            //Console.WriteLine("=========================================================");
            //Console.WriteLine("Completed Receiving all messages... Press any key to exit");
            //Console.WriteLine("=========================================================");

            //Console.ReadKey();

           
        }

        static async Task SendSessionMessagesAsync(int numberOfSessions, int messagesPerSession)
        {
            if (numberOfSessions == 0 || messagesPerSession == 0)
            {
                await Task.FromResult(false);
            }

            var random = new Random();
            for (int i = numberOfSessions - 1; i >= 0; i--)
            {
                var messagesToSend = new List<Message>();
                string sessionId = SessionPrefix + i;
                for (int j = 0; j < messagesPerSession; j++)
                {

                    var text = random.Next().ToString();

                    // Create a new message to send to the queue
                    var message = new Message(Encoding.UTF8.GetBytes(text));
                    // Assign a SessionId for the message
                    message.SessionId = sessionId;
                    messagesToSend.Add(message);

                    // Write the sessionId, body of the message to the console
                    Console.WriteLine($"Sending SessionId: {message.SessionId}, message: {text}");
                }

                // Send a batch of messages corresponding to this sessionId to the queue
                await messageSender.SendAsync(messagesToSend);
            }

            Console.WriteLine("=====================================");
            Console.WriteLine($"Sent {messagesPerSession} messages each for {numberOfSessions} sessions.");
            Console.WriteLine("=====================================");
        }

    }
}
