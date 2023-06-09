﻿using Microsoft.AspNetCore.Identity;

namespace WebChatTest.Models.Identity
{
    public class AppUser : IdentityUser
    {
        public virtual List<ChatRoom>? ChatRooms { get; set; }
        public virtual List<ChatRoom>? AdminsRooms { get; set; } 
    }
}
