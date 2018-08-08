using System.Linq;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Bootstrap.Service.NotificationService.Business.Senders;
using Bootstrap.Service.NotificationService.Models.Messages;
using Bootstrap.Service.NotificationService.Models.RequestModels;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Service.NotificationService.Business.Handlers.Implementations
{
    public abstract class EmailHandler : MessageHandler<EmailMessage>, IEmailHandler
    {
        protected EmailHandler(NotificationDbContext db, IEmailSender sender) : base(db, sender)
        {
        }

        public virtual async Task<SearchResponse<EmailMessage>> Search(EmailMessageSearchRequestModel model)
        {
            var query = Db.EmailMessages.Where(t => t.Email.Equals(model.Email)).OrderByDescending(a => a.Id);
            return new SearchResponse<EmailMessage>
            {
                Data = await query.Skip(model.PageIndex * model.PageSize).Take(model.PageSize).ToListAsync(),
                TotalCount = await query.CountAsync(),
                PageSize = model.PageSize,
                PageIndex = model.PageIndex
            };
        }
    }
}