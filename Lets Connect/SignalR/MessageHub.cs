using AutoMapper;
using Lets_Connect.Data;
using Lets_Connect.Data.DTO;
using Lets_Connect.Extensions;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
using Microsoft.AspNetCore.SignalR;

namespace Lets_Connect.SignalR
{
    public class MessageHub(IMessageRepository messageRepository, IUserRepository userRepository,
                            IMapper mapper, IHubContext<PresenceHub> presenseHub) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request.Query["user"];
            if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot join group");
            var groupName = GetGroupName(Context.User.GetUserName(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await messageRepository.GetMessageThread(Context.User.GetUserName(), otherUser!);
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);

        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User?.GetUserName() ?? throw new Exception("Cannot get user");
            if (username == createMessageDto.RecepientUserName.ToLower())
                throw new HubException("You cannot message yourself");

            var sender = await userRepository.GetUSerByNameAsync(username);
            var recepient = await userRepository.GetUSerByNameAsync(createMessageDto.RecepientUserName);

            if (recepient == null || sender == null || sender.UserName == null || recepient.UserName == null)
                throw new HubException("Cannot send message at this time");

            var message = new Message
            {
                Sender = sender,
                Recepient = recepient,
                SenderUserName = sender.UserName,
                ReceiverUserName = recepient.UserName,
                Context = createMessageDto.Content
            };

            var groupname = GetGroupName(sender.UserName, recepient.UserName);
            var group = await messageRepository.GetMessageGroup(groupname);

            if(group != null && group.Connections.Any(x => x.Username == recepient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenseTracker.GetConnectionsForUsers(recepient.UserName);
                if(connections != null && connections.Any())
                {
                    await presenseHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new
                    {
                        username = sender.UserName,
                        knownAs = sender.KnownAs
                    });
                }
            }

            messageRepository.AddMessage(message);
            if (await messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupname).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
        }

        private string GetGroupName(string caller, string? other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
        private async Task<Group> AddToGroup(string groupname)
        {
            var username = Context.User?.GetUserName() ?? throw new Exception("Cannot get username");
            var group = await messageRepository.GetMessageGroup(username);
            var connection = new Connection { ConnectionId = Context.ConnectionId, Username = username };

            if (group == null)
            { 
                group = new Group { Name = groupname };
                messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);
            if (await messageRepository.SaveAllAsync()) return group;
            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (connection != null && group != null)
            {
                messageRepository.RemoveConnection(connection);
                if (await messageRepository.SaveAllAsync()) return group;
            }
            throw new Exception("Failed to remove from Group");
        }
    }
}
