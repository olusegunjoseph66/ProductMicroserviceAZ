﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.ExternalServices.Configurations;
using Shared.ExternalServices.DTOs;
using Shared.ExternalServices.Interfaces;
using System.Text;

namespace Shared.ExternalServices.APIServices
{
    public class MessagingService : IMessagingService
    {

        private readonly MessagingServiceSetting _messagingSetting;
        private readonly IConfiguration _config;

        public MessagingService(IOptions<MessagingServiceSetting> messagingSetting, IConfiguration _config)
        {
            _messagingSetting = messagingSetting.Value;
            this._config = _config;
        }

        public async Task PublishTopicMessage(dynamic message, string subscriberName)
        {
            message.Id = Guid.NewGuid();

            var jsonMessage = JsonConvert.SerializeObject(message);
            var busMessage = new Message(Encoding.UTF8.GetBytes(jsonMessage))
            {
                PartitionKey = Guid.NewGuid().ToString(),
                Label = subscriberName
            };

            ISenderClient topicClient = new TopicClient(_config["MessagingServiceSetting:ConnectionString"], _config["MessagingServiceSetting:Product:TopicName"]);
            await topicClient.SendAsync(busMessage);
            Console.WriteLine($"Sent message to {topicClient.Path}");
            await topicClient.CloseAsync();

        }

        public async Task PublishTopicMessage(List<object> messages, string subscriberName)
        {
            List<Message> busMessages = new();
            var partitionId = Guid.NewGuid().ToString();
            messages.ForEach(x =>
            {
                var message = (dynamic)x;
                message.Id = Guid.NewGuid();
                var jsonMessage = JsonConvert.SerializeObject(message);
                var busMessage = new Message(Encoding.UTF8.GetBytes(jsonMessage))
                {
                    PartitionKey = partitionId,
                    Label = subscriberName,
                };
                busMessages.Add(busMessage);
            });

            ISenderClient topicClient = new TopicClient(_config["MessagingServiceSetting:ConnectionString"], _config["MessagingServiceSetting:Product:TopicName"]);
            //ISenderClient topicClient = new TopicClient(_messagingSetting.ConnectionString, _messagingSetting.TopicName);
            await topicClient.SendAsync(busMessages);
            Console.WriteLine($"Sent message to {topicClient.Path}");
            await topicClient.CloseAsync();

        }
    }
}
