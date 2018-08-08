using Bootstrap.Service.NotificationService.Models.Messages;

namespace Bootstrap.Service.NotificationService.Business.Senders
{
    public interface ISmsSender : IMessageSender<SmsMessage>
    {
    }
}
