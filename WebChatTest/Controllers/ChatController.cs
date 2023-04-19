using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebChatTest.Hubs;
using WebChatTest.Models;
using WebChatTest.Models.Identity;

namespace WebChatTest.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHubContext<ChatHub, IChatHub> _hubContext;

        public ChatController(ApplicationDbContext dbContext, UserManager<AppUser> userManager,
            IHubContext<ChatHub, IChatHub> hubContext)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var username = User.Identity.Name;
            var sender = await _userManager.FindByNameAsync(username);
            var chatRoomName = Guid.NewGuid().ToString();
            var room = await _dbContext.ChatRooms.AddAsync(new ChatRoom()
            {
                Name = chatRoomName,
                Admin = sender
            });
            await _dbContext.SaveChangesAsync();
            room.Entity.Users.Add(sender);
            await _dbContext.SaveChangesAsync();
            return Ok(chatRoomName);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddUserToRoom([FromBody]UserRoomModel info)
        {
            var username = User.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);
            var room = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.Name == info.ChatRoomName);
            if (room == null) {
                return BadRequest($"No room with name {info.ChatRoomName}.");
            }
            if (room.Admin?.Id != user.Id) {
                return Unauthorized("No rights to add users to this room.");
            }
            var userToAdd = await _userManager.FindByNameAsync(info.UserName);
            if (userToAdd == null)
            {
                return BadRequest($"No user with name {info.UserName}.");
            }
            if (!room.Users.Any(u => u.UserName == info.UserName))
                room.Users.Add(userToAdd);
            await _dbContext.SaveChangesAsync();
            foreach (var conn in ChatHub.GetConnectionsByUser(userToAdd.UserName))
            {
                await _hubContext.Clients.Client(conn).Notify($"{userToAdd.UserName} connected to group: {room.Name}");
            }
            await _hubContext.Clients.Group(room.Name).Notify($"{userToAdd.UserName} connected to group: {room.Name}");

            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveUserFromRoom([FromBody] UserRoomModel info)
        {
            var username = User.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);
            var room = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.Name == info.ChatRoomName);
            if (room == null)
            {
                return BadRequest($"No room with name {info.ChatRoomName}.");
            }
            if (room.Admin?.Id != user.Id && info.UserName != username)
            {
                return Unauthorized("No rights to add remove users from this room.");
            }
            var userToRemove = await _userManager.FindByNameAsync(info.UserName);
            if (userToRemove == null)
            {
                return BadRequest($"No user with name {info.UserName}.");
            }
            room.Users.Remove(userToRemove);
            await _dbContext.SaveChangesAsync();
            foreach (var conn in ChatHub.GetConnectionsByUser(userToRemove.UserName)) {
                await _hubContext.Groups.RemoveFromGroupAsync(conn, room.Name);
                await _hubContext.Clients.Client(conn).Notify($"{userToRemove.UserName} was removed from the group: {room.Name}");
            }
            await _hubContext.Clients.Group(room.Name).Notify($"{userToRemove.UserName} was removed from the group: {room.Name}");
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAvailableRooms()
        {
            var username = User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(username);
            var rooms = user?.ChatRooms?
                .Select(r => {
                    if (String.IsNullOrEmpty(r.DisplayName))
                    {
                        var someUserNamesFromGroup = r.Users.Take(3).Select(u => u.UserName);
                        r.DisplayName = String.Join(", ", someUserNamesFromGroup);
                    };
                    return r;
                }).ToList();
            return Ok(rooms ?? new List<ChatRoom>());
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRoomMessageHistory(string roomName)
        {
            var username = User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(username); 
            var room = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.Name == roomName);
            if (room == null)
            {
                return BadRequest($"No room with name {roomName}.");
            }
            if (!room.Users.Any(u => u.UserName == username))
                return Unauthorized($"User {username} has no access to room {room.Name}.");
            var result = _dbContext.Messages
                .Where(m => m.ChatRoom.Name == roomName)
                .OrderBy(m => m.Date)
                .Select(m => new Message() { 
                    Text = m.Text,
                    UserName = m.Sender.UserName
                })
                .ToList();
            return Ok(result);
        }
    }
}
