using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebChatTest.Models.Identity;

namespace WebChatTest.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public IdentityController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;

        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] LoginRegisterModel info)
        {
            var user = new AppUser { UserName = info.UserName };

            var userCreation = await _userManager.CreateAsync(user, info.Password);

            if (userCreation.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            else
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody] LoginRegisterModel info)
        {
            var user = await _userManager.FindByNameAsync(info.UserName);

            if (user == null)
                return BadRequest();

            var userSignIn = await _signInManager.PasswordSignInAsync(user, info.Password, false, false);

            if (userSignIn.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
