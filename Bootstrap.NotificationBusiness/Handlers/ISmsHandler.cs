using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Bootstrap.Service.NotificationService.Models.Messages;
using Bootstrap.Service.NotificationService.Models.RequestModels;

namespace Bootstrap.Service.NotificationService.Business.Handlers
{
    public interface ISmsHandler : IMessageHandler<SmsMessage>
    {
        Task<SearchResponse<SmsMessage>> Search(SmsSearchRequestModel model);
    }
}
