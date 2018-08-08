using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Bootstrap.Service.NotificationService.Models.Messages;

namespace Bootstrap.Service.NotificationService.Business.Handlers
{
    public interface IMessageHandler<in TMessage> where TMessage : Message
    {
        Task<BaseResponse> Send(TMessage message);
        Task<BaseResponse> Save(TMessage message);
    }
}