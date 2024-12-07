using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace bulksend
{
    public class RedisProducer
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly string _queueName;

        public RedisProducer(string redisConnectionString, string queueName)
        {
            _redis = ConnectionMultiplexer.Connect(redisConnectionString);
            _queueName = queueName;
        }

        public void PublishEmailBatch(IEnumerable<string> emails, string subject, string content)
        {
            var db = _redis.GetDatabase();

            // Create the email batch task as a JSON object
            var task = new
            {
                Emails = emails,
                Subject = subject,
                Content = content
            };

            var serializedTask = JsonSerializer.Serialize(task);

            // Push the task to the Redis list
            db.ListRightPush(_queueName, serializedTask);

            Console.WriteLine($"Published email batch to Redis: {serializedTask}");
        }
    }
}
