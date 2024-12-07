using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bulksend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Redis Email Consumer...");

            // Load configuration
            AppConfig config;
            Sender sender;
            Template template;
            List<Recipient> recipients;

            try
            {
                config = ConfigHelper.LoadFromJsonFile<AppConfig>("config.json");
                sender = ConfigHelper.LoadFromJsonFile<Sender>("sender.json");
                template = ConfigHelper.LoadFromJsonFile<Template>("template.json");
                recipients = ConfigHelper.LoadFromJsonFile<List<Recipient>>("recipients.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration or data: {ex.Message}");
                return;
            }

            // Publish tasks to Redis
            var producer = new RedisProducer(config.Redis.ConnectionString, config.Redis.QueueName);
            Parallel.ForEach(recipients, recipient =>
            {
                string personalizedContent = template.ContentTemplate.Replace("{{salutation}}", recipient.Salutation);

                producer.PublishEmailBatch(
                    new List<string> { recipient.Email },
                    template.Subject,
                    personalizedContent
                );

                Console.WriteLine($"Added task for: {recipient.Email}");
            });

            Console.WriteLine("Email tasks have been published to Redis!");

            // Start consuming tasks from Redis
            var consumer = new RedisConsumer(config.Redis.ConnectionString, config.Redis.QueueName, config.SendGrid.ApiKey);
            await consumer.StartConsuming(
                sender.SenderEmail,
                sender.SenderName,
                template.Subject,
                template.ContentTemplate
            );

            Console.WriteLine("Redis Email Consumer process completed.");
        }
    }
}
