using Bootstrap.Service.NotificationService.Models.Messages;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Service.NotificationService.Business
{
    public abstract class NotificationDbContext : DbContext
    {
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<SmsMessage> SmsMessages { get; set; }
        public DbSet<WeChatTemplateMessage> WeChatTemplateMessages { get; set; }
    }
}