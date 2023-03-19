using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        public ChatController(ApplicationDbContext dbContext, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var username = User.Identity.Name;
            var sender = await _userManager.FindByNameAsync(username);
            var chatRoomName = Guid.NewGuid().ToString();
            try
            {
                var room = await _dbContext.ChatRooms.AddAsync(new ChatRoom()
                {
                    Name = chatRoomName,
                    DisplayName = chatRoomName
                });
                await _dbContext.SaveChangesAsync();
                room.Entity.Users.Add(sender);
            }
            catch (Exception e) { 
                return BadRequest("Error creating chat room.");
            }
            await _dbContext.SaveChangesAsync();
            return Ok(chatRoomName);
        }

        [Authorize]
        [HttpPost]
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
    }
}
