using System.Linq;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Bootstrap.Service.NotificationService.Business.Senders;
using Bootstrap.Service.NotificationService.Models.Messages;
using Bootstrap.Service.NotificationService.Models.RequestModels;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Service.NotificationService.Business.Handlers.Implementations
{
    public abstract class WeChatTemplateMessageHandler : MessageHandler<WeChatTemplateMessage>, IWeChatTemplateMessageHandler
    {
        protected WeChatTemplateMessageHandler(NotificationDbContext db, IWeChatTemplateMessageSender sender) : base(db, sender)
        {
        }

        public virtual async Task<SearchResponse<WeChatTemplateMessage>> Search(WeChatTemplateMessageSearchRequestModel model)
        {
            var query = Db.WeChatTemplateMessages.Where(t => t.OpenId.Equals(model.OpenId)).OrderByDescending(a => a.Id);
            return new SearchResponse<WeChatTemplateMessage>
            {
                Data = await query.Skip(model.PageIndex * model.PageSize).Take(model.PageSize).ToListAsync(),
                TotalCount = await query.CountAsync(),
                PageSize = model.PageSize,
                PageIndex = model.PageIndex
            };
        }
    }
}
