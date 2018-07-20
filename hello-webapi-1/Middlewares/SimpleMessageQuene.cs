using System;
using System.Linq;
using System.Collections;
using ServiceStack.Redis;
using System.Collections.Generic;


namespace hello_webapi.Middlewares
{
    public class SimpleMessageQueue
    {
        private string _connectionString;
        private readonly BasicRedisClientManager _clientManager;
        public SimpleMessageQueue(string connectionString)
        {
            _connectionString = connectionString;
            _clientManager = new BasicRedisClientManager(_connectionString);
        }

        public void Push(string channel, string messsage)
        {
            using (var client = _clientManager.GetClient())
            {
                client.PushItemToList(channel, messsage);
            }
        }

        public void Push(string channel, IEnumerable<string> messages)
        {
            using (var client = _clientManager.GetClient())
            {
                client.AddRangeToList(channel, messages.ToList());
            }
        }

        public string Pull(string channel,TimeSpan interval)
        {
            using (var client = _clientManager.GetClient())
            {
                return client.BlockingDequeueItemFromList(channel,interval);
            }
        }
    }
}