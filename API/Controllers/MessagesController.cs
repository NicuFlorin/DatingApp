using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        private readonly IMapper mapper;
        public MessagesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;

        }

        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var UserName = User.GetUsername();

            if (UserName == createMessageDto.RecipientUsername.ToLower())
            {
                return BadRequest("You cannot send messages to yourself");

            }

            var sender = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(UserName);
            var recipient = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null)
            {
                return NotFound();
            }

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            this.unitOfWork.MessageRepository.AddMessage(message);

            if (await this.unitOfWork.Complete())
            {
                return Ok(this.mapper.Map<MessageDto>(message));
            }
            return BadRequest("Failed to send message");

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await this.unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }


        [HttpDelete("{Id}")]
        public async Task<ActionResult> DeleteMessage(int Id)
        {
            var UserName = User.GetUsername();

            var message = await this.unitOfWork.MessageRepository.GetMessage(Id);

            if (message.Sender.UserName != UserName && message.Recipient.UserName != UserName)
            {
                return Unauthorized();
            }
            if (message.Sender.UserName == UserName) message.SenderDeleted = true;

            if (message.Recipient.UserName == UserName) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                this.unitOfWork.MessageRepository.DeleteMessage(message);

            }

            if (await this.unitOfWork.Complete())
            {
                return Ok();
            }

            return BadRequest("Problem deleting the message");

        }




    }
}