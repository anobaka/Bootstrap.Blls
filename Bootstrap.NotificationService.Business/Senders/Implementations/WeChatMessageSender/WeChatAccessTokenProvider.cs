using System.Threading.Tasks;

namespace Bootstrap.Service.NotificationService.Business.Senders.Implementations.WeChatMessageSender
{
    public class WeChatAccessTokenProvider : IWeChatAccessTokenProvider
    {
        public async Task<string> GetAccessToken()
        {
            return await Task.FromResult((string) null);
        }
    }
}
