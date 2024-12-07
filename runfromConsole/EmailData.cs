namespace bulksend
{
    public class Sender
    {
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
    }
}

namespace bulksend
{
    public class Template
    {
        public string Subject { get; set; }
        public string ContentTemplate { get; set; }
    }
}



namespace bulksend
{
    public class Recipient
    {
        public string Email { get; set; }
        public string Salutation { get; set; }
    }
}
