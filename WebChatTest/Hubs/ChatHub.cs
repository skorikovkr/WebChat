using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebChatTest.Models.Identity;
using static WebChatTest.Infrastructure.ConnectionMapper;

namespace WebChatTest.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();
        private readonly ApplicationDbContext _dbContext;

        public static IEnumerable<string> GetConnectionsByUser(string username) {
            return _connections.GetConnections(username);
        }

        public ChatHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ConnectToRoom(string roomName)
        {
            var username = Context.User.Identity.Name;
            var room = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.Name == roomName);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Notify", $"No room with name {roomName}");
                return;
            }
            if (!room.Users.Any(u => u.UserName == username))
            {
                await Clients.Caller.SendAsync("Notify", $"User {username} has no access to room {room.Name}.");
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }
        public Task LeaveToRoom(string roomName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task SendMessageToRoom(string roomName, string message)
        {
            var username = Context.User.Identity.Name;
            var room = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.Name == roomName);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Notify", $"No room with name {roomName}");
                return;
            }
            if (!room.Users.Any(u => u.UserName == username))
            {
                await Clients.Caller.SendAsync("Notify", $"User {username} has no access to room {room.Name}.");
                return;
            }
            await Clients.Group(roomName).SendAsync("RecieveMessage", username, message);
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
