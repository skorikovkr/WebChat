using WebChatTest.Models.Identity;

namespace WebChatTest.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public virtual ChatRoom ChatRoom { get; set; }
        public virtual AppUser Sender { get; set; }
    }
}
