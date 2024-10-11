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
    public class MessagesController(IUnitOfWork unitOfWork
        , IMapper mapper): BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserName();
            if (username == createMessageDto.RecepientUserName.ToLower())
                return BadRequest("You cannot message yourself");

            var sender = await unitOfWork.UserRepository.GetUSerByNameAsync(username);
            var recepient = await unitOfWork.UserRepository.GetUSerByNameAsync(createMessageDto.RecepientUserName);

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

            unitOfWork.MessageRepository.AddMessage(message);
            if(await unitOfWork.Complete()) return Ok(mapper.Map<MessageDto>(message));
            return BadRequest("Failed to save Message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();
            var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUserName();
            return Ok(await unitOfWork.MessageRepository.GetMessageThread(currentUsername, username));
        }
        
        [HttpDelete("id")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();

            var message = await unitOfWork.MessageRepository.GetMessage(id);
            if (message == null) return BadRequest("Cannot delete the message");
            if (message.SenderUserName != username && message.ReceiverUserName != username) return Forbid();

            if(message.SenderUserName == username) message.SenderDeleted = true;
            if(message.ReceiverUserName == username) message.ReceipientDeleted = true;

            if(message is { SenderDeleted: true, ReceipientDeleted: true })
                unitOfWork.MessageRepository.DeleteMessage(message);

            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
        }

    }
}
