using AutoMapper;
using Lets_Connect.Data.DTO;
using Lets_Connect.Extensions;
using Lets_Connect.Helpers;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lets_Connect.Controllers
{
    [Authorize]
    public class MessagesController(IMessageRepository messageRepository, IUserRepository userRepository
        , IMapper mapper): BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserName();
            if (username == createMessageDto.RecepientUserName.ToLower())
                return BadRequest("You cannot message yourself");

            var sender = await userRepository.GetUSerByNameAsync(username);
            var recepient = await userRepository.GetUSerByNameAsync(createMessageDto.RecepientUserName);

            if (recepient == null || sender == null || sender.UserName == null || recepient.UserName == null)
                return BadRequest("Cannot send message");

            var message = new Message
            {
                Sender = sender,
                Recepient = recepient,
                SenderUserName = sender.UserName,
                ReceiverUserName = recepient.UserName,
                Context = createMessageDto.Content
            };

            messageRepository.AddMessage(message);
            if(await messageRepository.SaveAllAsync()) return Ok(mapper.Map<MessageDto>(message));
            return BadRequest("Failed to save Message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();
            var messages = await messageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUserName();
            return Ok(await messageRepository.GetMessageThread(currentUsername, username));
        }
        
        [HttpDelete("id")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();

            var message = await messageRepository.GetMessage(id);
            if (message == null) return BadRequest("Cannot delete the message");
            if (message.SenderUserName != username && message.ReceiverUserName != username) return Forbid();

            if(message.SenderUserName == username) message.SenderDeleted = true;
            if(message.ReceiverUserName == username) message.ReceipientDeleted = true;

            if(message is { SenderDeleted: true, ReceipientDeleted: true })
                messageRepository.DeleteMessage(message);

            if (await messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }

    }
}
