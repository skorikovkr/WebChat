using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebChatTest.Models.Identity;

namespace WebChatTest.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }

        [JsonIgnore]
        public virtual ChatRoom ChatRoom { get; set; }

        [JsonIgnore]
        public virtual AppUser Sender { get; set; }


        [NotMapped]
        public string UserName { get; set; }

        [NotMapped]
        public string UserId { get; set; }

        [NotMapped]
        public string ChatRoomName { get; set; }
    }
}
