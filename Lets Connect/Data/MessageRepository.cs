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
        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Connection?> GetConnection(string connectionId)
        {
            return await context.Connections.FindAsync(connectionId);
        }

        public async Task<Group?> GetGroupForConnection(string connectionId)
        {
            return await context.Groups.Include(x => x.Connections)
                                       .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                                       .FirstOrDefaultAsync();
        }

        public async Task<Message?> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<Group?> GetMessageGroup(string groupname)
        {
            return await context.Groups.Include(x => x.Connections)
                                       .FirstOrDefaultAsync(x => x.Name == groupname);
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
            var query = context.Messages
                .Where(x => x.ReceiverUserName == currentUserName && x.ReceipientDeleted == false
                && x.SenderUserName == recepientUserName || x.SenderDeleted == false &&
                x.SenderUserName == currentUserName && x.ReceiverUserName == recepientUserName)
                .OrderBy(x => x.MessageSent)
                .AsQueryable();

            var unreadMessages = query.Where(x => x.DateRead == null && x.ReceiverUserName == currentUserName).ToList();
            if (unreadMessages.Count > 0)
            {
                unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
            }

            return await query.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection);
        }
    }
}
