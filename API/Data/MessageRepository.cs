using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public void AddGroup(Group group)
        {
            this.context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);

        }

        public void DeleteMessage(Message message)
        {
            this.context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await this.context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await this.context
               .Groups
               .Include(c => c.Connections)
               .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
               .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int Id)
        {
            return await this.context.Messages
                .Include(u => u.Sender)
                .Include(u => u.Recipient)
                .SingleOrDefaultAsync(m => m.Id == Id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await this.context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PageList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = this.context.Messages
                .OrderByDescending(message => message.MessageSent)
                .ProjectTo<MessageDto>(this.mapper.ConfigurationProvider)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(user => user.RecipientUsername == messageParams.Username && user.RecipientDeleted == false),
                "Outbox" => query.Where(user => user.SenderUsername == messageParams.Username && user.SenderDeleted == false),


                _ => query.Where(user => user.RecipientUsername == messageParams.Username && user.RecipientDeleted == false && user.DateRead == null)
            };


            return await PageList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await this.context.Messages
                  .Include(user => user.Sender).ThenInclude(p => p.Photos)
                  .Include(user => user.Recipient).ThenInclude(p => p.Photos)
                 .Where(message => message.Recipient.UserName == currentUsername
                         && message.RecipientDeleted == false
                         && message.Sender.UserName == recipientUsername
                         || message.Recipient.UserName == recipientUsername
                         && message.Sender.UserName == currentUsername
                         && message.SenderDeleted == false)
                 .OrderBy(message => message.MessageSent)
                 .ProjectTo<MessageDto>(this.mapper.ConfigurationProvider)
                 .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null
                && m.RecipientUsername == currentUsername).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }

            }

            return messages;

        }

        public void RemoveConnection(Connection connection)
        {
            this.context.Connections.Remove(connection);
        }


    }
}