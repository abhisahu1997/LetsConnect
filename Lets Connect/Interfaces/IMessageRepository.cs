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
        Task<bool> SaveAllAsync();

    }
}
