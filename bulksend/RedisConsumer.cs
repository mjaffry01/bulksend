using Polly;
using SendGrid.Helpers.Mail;
using SendGrid;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public class RedisConsumer
{
    private readonly ConnectionMultiplexer _redis;
    private readonly string _queueName;
    private readonly string _sendGridApiKey;

    public RedisConsumer(string redisConnectionString, string queueName, string sendGridApiKey)
    {
        _redis = ConnectionMultiplexer.Connect(redisConnectionString);
        _queueName = queueName;
        _sendGridApiKey = sendGridApiKey;
    }

    public async Task StartConsuming(string senderEmail, string senderName, string subject, string content)
    {
        var retryPolicy = Policy
            .Handle<RedisConnectionException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} for Redis operation due to {exception.Message}");
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            var db = _redis.GetDatabase();
            var serializedTask = await db.ListLeftPopAsync(_queueName);

            if (!serializedTask.IsNullOrEmpty)
            {
                // Call ProcessTaskAsync to process the task
                await ProcessTaskAsync(serializedTask, senderEmail, senderName, subject, content);
            }
        });
    }

    private async Task ProcessTaskAsync(string serializedTask, string senderEmail, string senderName, string subject, string content)
    {
        try
        {
            // Deserialize the task
            var task = JsonSerializer.Deserialize<EmailTask>(serializedTask);
            if (task != null && task.Emails != null && task.Emails.Length > 0)
            {
                // Send emails
                await SendEmails(task.Emails, senderEmail, senderName, task.Subject ?? subject, task.Content ?? content);
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to deserialize task: {ex.Message}");
            // Optionally log the failure or push the task to a Dead Letter Queue (DLQ)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing task: {ex.Message}");
        }
    }

    private async Task SendEmails(string[] emails, string senderEmail, string senderName, string subject, string content)
    {
        var client = new SendGridClient(_sendGridApiKey);
        var from = new EmailAddress(senderEmail, senderName);

        foreach (var email in emails)
        {
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var response = await client.SendEmailAsync(message);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Email sent to {email}");
            }
            else
            {
                Console.WriteLine($"Failed to send email to {email}: {response.StatusCode}");
            }
        }
    }
}

public class EmailTask
{
    public string[] Emails { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
}
