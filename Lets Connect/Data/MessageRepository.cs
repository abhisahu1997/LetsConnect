using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lets_Connect.Data.DTO;
using Lets_Connect.Helpers;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
using Microsoft.EntityFrameworkCore;

namespace Lets_Connect.Data
{
    public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
    {
        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Message?> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(x => x.Recepient.UserName == messageParams.UserName && x.ReceipientDeleted == false),
                "Outbox" => query.Where(x => x.Sender.UserName == messageParams.UserName && x.SenderDeleted == false),
                _ => query.Where(x => x.Recepient.UserName == messageParams.UserName && x.DateRead == null && x.ReceipientDeleted == false),
            };

            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recepientUserName)
        {
            var messages = await context.Messages.Include(x => x.Sender).ThenInclude(x => x.Photos)
                .Include(x => x.Recepient).ThenInclude(x => x.Photos)
                .Where(x => x.ReceiverUserName == currentUserName && x.ReceipientDeleted == false
                && x.SenderUserName == recepientUserName ||  x.SenderDeleted == false &&
                x.SenderUserName == currentUserName && x.ReceiverUserName == recepientUserName )
                .OrderBy(x => x.MessageSent).ToListAsync();

            var unreadMessages = messages.Where(x => x.DateRead == null && x.ReceiverUserName == currentUserName).ToList();
            if (unreadMessages.Count > 0)
            {
                unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
                await context.SaveChangesAsync();
            }

            return mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}
