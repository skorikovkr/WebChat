﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
                List<Claim> claims = new List<Claim>();

                claims.Add(new Claim(ClaimTypes.Name, info.UserName));

                foreach (var claim in claims)
                    await _userManager.AddClaimAsync(user, claim);
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
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, info.UserName) };
                var jwt = new JwtSecurityToken(
                        issuer: "WebChatServer",
                        audience: "WebChatClient",
                        claims: claims,
                        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey_ForWebChat123123")),
                            SecurityAlgorithms.HmacSha256));
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                return Ok(encodedJwt);
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
