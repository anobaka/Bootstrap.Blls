using System;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Bootstrap.Service.NotificationService.Business.Senders;
using Bootstrap.Service.NotificationService.Models.Constants;
using Bootstrap.Service.NotificationService.Models.Messages;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Service.NotificationService.Business.Handlers.Implementations
{
    public abstract class MessageHandler<TMessage> : IMessageHandler<TMessage> where TMessage : Message
    {
        protected readonly NotificationDbContext Db;
        protected readonly IMessageSender<TMessage> Sender;

        protected MessageHandler(NotificationDbContext db, IMessageSender<TMessage> sender)
        {
            Db = db;
            Sender = sender;
        }

        /// <summary>
        /// Send message.
        /// </summary>
        /// <param name="message">All fields of message should be correct to ensure ef updates the correct data.</param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> Send(TMessage message)
        {
            var rsp = await Sender.Send(message);
            if (Db.Entry(message).State == EntityState.Detached)
            {
                Db.Attach(message);
            }
            message.TryTimes++;
            if (rsp.Code != 0)
            {
                message.Status = MessageStatus.Failed;
            }
            else
            {
                message.Status = MessageStatus.Succeed;
                message.SendDt = DateTime.Now;
            }
            await Db.SaveChangesAsync();
            return rsp;
        }

        public virtual async Task<BaseResponse> Save(TMessage message)
        {
            if (message.Id == 0)
            {
                Db.Add(message);
                await Db.SaveChangesAsync();
            }
            return new BaseResponse();
        }
    }
}