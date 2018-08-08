using System.Threading.Tasks;

namespace Bootstrap.Service.NotificationService.Business.Senders.Implementations.WeChatMessageSender
{
    public interface IWeChatAccessTokenProvider
    {
        Task<string> GetAccessToken();
    }
}
