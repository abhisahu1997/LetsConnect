using Lets_Connect.Data.DTO;
using Lets_Connect.Helpers;
using Lets_Connect.Model;

namespace Lets_Connect.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);  
        Task<Message?> GetMessage(int id);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recepientUserName);
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection?> GetConnection(string connectionId);
        Task<Group?> GetMessageGroup(string groupname);
        Task<Group?> GetGroupForConnection(string connectionId);

    }
}
