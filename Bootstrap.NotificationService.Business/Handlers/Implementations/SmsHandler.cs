using System.Linq;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Bootstrap.Service.NotificationService.Business.Senders;
using Bootstrap.Service.NotificationService.Models.Messages;
using Bootstrap.Service.NotificationService.Models.RequestModels;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Service.NotificationService.Business.Handlers.Implementations
{
    public abstract class SmsHandler : MessageHandler<SmsMessage>, ISmsHandler
    {
        protected SmsHandler(NotificationDbContext db, ISmsSender sender) : base(db, sender)
        {
        }

        public virtual async Task<SearchResponse<SmsMessage>> Search(SmsSearchRequestModel model)
        {
            var query = Db.SmsMessages.Where(t => t.Mobile.Equals(model.Mobile)).OrderByDescending(a => a.Id);
            return new SearchResponse<SmsMessage>
            {
                Data = await query.Skip(model.PageIndex * model.PageSize).Take(model.PageSize).ToListAsync(),
                TotalCount = await query.CountAsync(),
                PageSize = model.PageSize,
                PageIndex = model.PageIndex
            };
        }
    }
}