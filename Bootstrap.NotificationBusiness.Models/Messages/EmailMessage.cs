namespace Bootstrap.Service.NotificationService.Models.Messages
{
    public class EmailMessage : Message
    {
        public string Email { get; set; }
        public string Content { get; set; }
    }
}
