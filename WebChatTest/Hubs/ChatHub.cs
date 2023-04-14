using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebChatTest.Models;
using WebChatTest.Models.Identity;
using static WebChatTest.Infrastructure.ConnectionMapper;

namespace WebChatTest.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatHub>
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;

        public static IEnumerable<string> GetConnectionsByUser(string username)
        {
            return _connections.GetConnections(username);
        }

        public ChatHub(ApplicationDbContext dbContext, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task ConnectToRoom(string roomName)
        {
            var username = Context.User.Identity.Name;
            var room = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.Name == roomName);
            if (room == null)
            {
                await Clients.Caller.Notify($"No room with name {roomName}");
                return;
            }
            if (!room.Users.Any(u => u.UserName == username))
            {
                await Clients.Caller.Notify($"User {username} has no access to room {room.Name}.");
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task LeaveFromRoom(string roomName)
        {
            var username = Context.User.Identity.Name;
            foreach (var connId in _connections.GetConnections(username))
                await Groups.RemoveFromGroupAsync(connId, roomName);
        }

        public async Task SendMessageToRoom(string roomName, string message)
        {
            var username = Context.User.Identity.Name;
            var room = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.Name == roomName);
            if (room == null)
            {
                await Clients.Caller.Notify($"No room with name {roomName}");
                return;
            }
            if (!room.Users.Any(u => u.UserName == username))
            {
                await Clients.Caller.Notify($"User {username} has no access to room {room.Name}.");
                return;
            }
            await Clients.Group(roomName).RecieveMessage(username, message);
            var user = await _userManager.FindByNameAsync(username);
            await _dbContext.Messages.AddAsync(new Message()
            {
                Text = message,
                ChatRoom = room,
                Sender = user,
                Date = DateTime.Now,
                UserName = username
            });
            await _dbContext.SaveChangesAsync();
        }

        public override async Task OnConnectedAsync()
        {
            string name = Context.User.Identity.Name;
            _connections.Add(name, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string name = Context.User.Identity.Name;
            _connections.Remove(name, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
