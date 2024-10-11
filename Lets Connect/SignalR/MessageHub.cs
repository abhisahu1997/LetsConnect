using AutoMapper;
using Lets_Connect.Data;
using Lets_Connect.Data.DTO;
using Lets_Connect.Extensions;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
using Microsoft.AspNetCore.SignalR;

namespace Lets_Connect.SignalR
{
    public class MessageHub(IUnitOfWork unitOfWork,
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

            var messages = await unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUserName(), otherUser!);

            if (unitOfWork.HasChanges()) await unitOfWork.Complete();

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

            var sender = await unitOfWork.UserRepository.GetUSerByNameAsync(username);
            var recepient = await unitOfWork.UserRepository.GetUSerByNameAsync(createMessageDto.RecepientUserName);

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
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupname);

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

            unitOfWork.MessageRepository.AddMessage(message);
            if (await unitOfWork.Complete())
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
            var group = await unitOfWork.MessageRepository.GetMessageGroup(username);
            var connection = new Connection { ConnectionId = Context.ConnectionId, Username = username };

            if (group == null)
            { 
                group = new Group { Name = groupname };
                unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);
            if (await unitOfWork.Complete()) return group;
            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (connection != null && group != null)
            {
                unitOfWork.MessageRepository.RemoveConnection(connection);
                if (await unitOfWork.Complete()) return group;
            }
            throw new Exception("Failed to remove from Group");
        }
    }
}
