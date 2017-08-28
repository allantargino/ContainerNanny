using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Producer
{
    public static class Settings
    {
        public static string ConnectionString = "<YOUR SERVICE BUS CONNECTION STRING>";

        public static string QueueName = "<YOUR QUEUE NAME>";

        public static int MessageDelay = 200;
    }
}
