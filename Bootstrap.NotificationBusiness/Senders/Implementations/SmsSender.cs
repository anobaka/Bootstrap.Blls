using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Bootstrap.Service.NotificationService.Models.Messages;

namespace Bootstrap.Service.NotificationService.Business.Senders.Implementations
{
    public class SmsSender : ISmsSender
    {
        public async Task<BaseResponse> Send(SmsMessage message)
        {
            return await Task.FromResult(new BaseResponse());
        }
    }
}
