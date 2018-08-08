using Bootstrap.Service.NotificationService.Models.Messages;

namespace Bootstrap.Service.NotificationService.Business.Senders
{
    public interface IEmailSender : IMessageSender<EmailMessage>
    {
    }
}
