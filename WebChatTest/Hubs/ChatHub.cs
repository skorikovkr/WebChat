using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebChatTest.Models;
using WebChatTest.Models.Identity;

namespace WebChatTest.Hubs
{
    public class ChatHub : Hub
    {

        private protected ApplicationDbContext _dbContext;

        public ChatHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        public async Task SendMessage(string username, string message, string chatRoomName)
        {
            //await Clients.Group(chatRoomName).SendAsync("RecieveMessage", username, message);

            //await _dbContext.Messages.AddAsync(new Message()
            //{
            //    Text = message,
            //    Sender = sender,
            //    ChatRoom = 
            //});
            //await _dbContext.SaveChangesAsync();
        }
    }
}
