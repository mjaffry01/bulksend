namespace bulksend
{
    public class AppConfig
    {
        public RedisConfig Redis { get; set; }
        public SendGridConfig SendGrid { get; set; }
    }

    public class RedisConfig
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }

    public class SendGridConfig
    {
        public string ApiKey { get; set; }
    }
}
