using System;
using System.ComponentModel.DataAnnotations.Schema;
using Bootstrap.Service.NotificationService.Models.Constants;

namespace Bootstrap.Service.NotificationService.Models.Messages
{
    public abstract class Message
    {
        public int Id { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.ToBeSent;
        public int TryTimes { get; set; }
        public DateTime CreateDt { get; set; } = DateTime.Now;
        public DateTime? ScheduleDt { get; set; }
        public DateTime? SendDt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateDt { get; set; } = DateTime.Now;
    }
}