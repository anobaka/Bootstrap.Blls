using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Bootstrap.Service.NotificationService.Models.Messages;

namespace Bootstrap.Service.NotificationService.Business.Senders.Implementations.WeChatMessageSender
{
    public class WeChatMessageSender : IWeChatTemplateMessageSender
    {
        public async Task<BaseResponse> Send(WeChatTemplateMessage message)
        {
            return await Task.FromResult(new BaseResponse());
        }
    }
}
