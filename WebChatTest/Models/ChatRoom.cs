using WebChatTest.Models.Identity;

namespace WebChatTest.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public virtual List<AppUser> Users { get; set; } = new List<AppUser>();
    }
}
