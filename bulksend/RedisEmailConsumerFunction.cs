using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace bulksend
{
    public static class RedisEmailConsumerFunction
    {
        [FunctionName("RedisEmailConsumerHttpTrigger")]
        public static async Task<IActionResult> RunHttpTrigger(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"[{DateTime.Now}] HTTP Trigger: RedisEmailConsumer invoked.");
            Console.WriteLine($"[{DateTime.Now}] HTTP Trigger: RedisEmailConsumer invoked.");

            AppConfig config;
            Sender sender;
            Template template;
            List<Recipient> recipients;

            try
            {
                log.LogInformation($"[{DateTime.Now}] Loading configuration and JSON data files...");
                Console.WriteLine($"[{DateTime.Now}] Loading configuration and JSON data files...");

                // Load configuration and data from JSON files
                config = ConfigHelper.LoadFromJsonFile<AppConfig>("config.json");
                sender = ConfigHelper.LoadFromJsonFile<Sender>("sender.json");
                template = ConfigHelper.LoadFromJsonFile<Template>("template.json");
                recipients = ConfigHelper.LoadFromJsonFile<List<Recipient>>("recipients.json");

                log.LogInformation($"[{DateTime.Now}] Successfully loaded configuration and JSON data files.");
                Console.WriteLine($"[{DateTime.Now}] Successfully loaded configuration and JSON data files.");
            }
            catch (Exception ex)
            {
                log.LogError($"[{DateTime.Now}] Error loading configuration or JSON data files: {ex.Message}");
                Console.WriteLine($"[{DateTime.Now}] Error loading configuration or JSON data files: {ex.Message}");
                return new BadRequestObjectResult($"Error loading configuration or JSON data files: {ex.Message}");
            }

            try
            {
                log.LogInformation($"[{DateTime.Now}] Publishing tasks to Redis for {recipients.Count} recipients...");
                Console.WriteLine($"[{DateTime.Now}] Publishing tasks to Redis for {recipients.Count} recipients...");

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

                    log.LogInformation($"[{DateTime.Now}] Task published to Redis for recipient: {recipient.Email}");
                    Console.WriteLine($"[{DateTime.Now}] Task published to Redis for recipient: {recipient.Email}");
                });

                log.LogInformation($"[{DateTime.Now}] Successfully published tasks to Redis.");
                Console.WriteLine($"[{DateTime.Now}] Successfully published tasks to Redis.");

                // Start consuming tasks from Redis
                log.LogInformation($"[{DateTime.Now}] Starting to process tasks from Redis...");
                Console.WriteLine($"[{DateTime.Now}] Starting to process tasks from Redis...");

                var consumer = new RedisConsumer(config.Redis.ConnectionString, config.Redis.QueueName, config.SendGrid.ApiKey);
                await consumer.StartConsuming(
                    sender.SenderEmail,
                    sender.SenderName,
                    template.Subject,
                    template.ContentTemplate
                );

                log.LogInformation($"[{DateTime.Now}] Redis email consumer completed successfully.");
                Console.WriteLine($"[{DateTime.Now}] Redis email consumer completed successfully.");
                return new OkObjectResult("Redis email consumer process completed successfully.");
            }
            catch (Exception ex)
            {
                log.LogError($"[{DateTime.Now}] Error processing tasks from Redis: {ex.Message}");
                Console.WriteLine($"[{DateTime.Now}] Error processing tasks from Redis: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
