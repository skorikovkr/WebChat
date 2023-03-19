using System.ComponentModel.DataAnnotations;

namespace WebChatTest.Models.Identity
{
    public class LoginRegisterModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
