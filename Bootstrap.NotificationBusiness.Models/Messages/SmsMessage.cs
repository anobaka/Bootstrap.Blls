namespace Bootstrap.Service.NotificationService.Models.Messages
{
    public class SmsMessage : Message
    {
        public string Mobile { get; set; }
        public string Content { get; set; }
    }
}
