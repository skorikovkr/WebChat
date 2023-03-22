using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ApplicationDbContext dbContext, UserManager<AppUser> userManager,
            IHubContext<ChatHub> hubContext)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(string roomName = "")
        {
            var username = User.Identity.Name;
            var sender = await _userManager.FindByNameAsync(username);
            var chatRoomName = Guid.NewGuid().ToString();
            var room = await _dbContext.ChatRooms.AddAsync(new ChatRoom()
            {
                Name = chatRoomName,
                DisplayName = roomName == "" ? chatRoomName : roomName
            });
            await _dbContext.SaveChangesAsync();
            room.Entity.Users.Add(sender);
            await _dbContext.SaveChangesAsync();
            return Ok(chatRoomName);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddUserToRoom([FromBody]AddingUserToRoomModel info)
        {
            var room = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.Name == info.ChatRoomName);
            if (room == null) {
                return BadRequest($"No room with name {info.ChatRoomName}.");
            }
            var userToAdd = await _userManager.FindByNameAsync(info.UserName);
            if (userToAdd == null)
            {
                return BadRequest($"No user with name {info.UserName}.");
            }
            if (!room.Users.Any(u => u.UserName == info.UserName))
                room.Users.Add(userToAdd);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAvailableRooms()
        {
            var username = User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(username);
            return Ok(user.ChatRooms);
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
