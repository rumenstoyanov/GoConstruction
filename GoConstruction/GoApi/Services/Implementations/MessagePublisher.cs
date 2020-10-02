using GoApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace GoApi.Services.Implementations
{
    public class MessagePublisher : IMessagePublisher
    {

        private readonly IQueueClient _queueClient;

        public MessagePublisher(IQueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        public Task Publish<T>(T obj) where T : class
        {
            var json = JsonConvert.SerializeObject(obj);
            var message = new Message(Encoding.UTF8.GetBytes(json));

            return _queueClient.SendAsync(message);
        }
    }
}
