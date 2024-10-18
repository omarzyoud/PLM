using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PLM.api.Data;
using PLM.api.Models.Domain;
using PLM.api.Models.DTO;
using PLM.api.Repositories;
using System.Security.Claims;


namespace PLM.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly PLMDbContext pLMDbContext;
        private readonly ITokenRepository tokenRepository;
        private readonly ICustomEmailSender emailSender;
        public AuthController(UserManager<IdentityUser> userManager, PLMDbContext pLMDbContext, ITokenRepository tokenRepository, ICustomEmailSender emailSender)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.pLMDbContext = pLMDbContext;
            this.emailSender = emailSender;
            //test git

        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO requestDTO)
        {
            var identityUser = new IdentityUser
            {
                UserName = requestDTO.Email,
                Email = requestDTO.Email,

            };
            var identityResult = await userManager.CreateAsync(identityUser, requestDTO.Password);
            if (identityResult.Succeeded)
            {
                var user = new User
                {
                    FullName = requestDTO.FullName,
                    Type = UserType.User,
                    Email = requestDTO.Email,

                };
                pLMDbContext.Users.Add(user);
                await pLMDbContext.SaveChangesAsync();
                identityResult = await userManager.AddToRolesAsync(identityUser, new string[] { "User" });
                if (identityResult.Succeeded)
                {
                    return Ok("succecfull registered, please login");
                }
            }
            else
            {
                return BadRequest("Not saved");
            }
            return BadRequest("something went wrong");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user = await userManager.FindByEmailAsync(loginRequestDTO.Email);
            if (user != null)
            {
                var checkPassword = await userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
                if (checkPassword)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    User usr = pLMDbContext.Users.FirstOrDefault(u => u.Email == loginRequestDTO.Email);
                    int usrid = usr.Id;
                    string name = usr.FullName;
                    if (roles != null)
                    {


                        var jwttoken = tokenRepository.CreateJWTToken(user, roles.ToList());
                        var response = new LoginResponseDTO
                        {
                            JwtToken = jwttoken,
                            Roles = roles.ToList(),
                            UserId = usrid,
                            Name = name,


                        };

                        return Ok(response);
                    }
                }
            }

            return BadRequest("Username or password incorrect");
        }
        [HttpPost]
        [Route("AddAmin")]
        public async Task<IActionResult> AddAdmin([FromBody] AddAdminDTO model)

        {
            var identityUser = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,

            };
            var identityResult = await userManager.CreateAsync(identityUser, model.Password);
            if (identityResult.Succeeded)
            {
                var user = new User
                {
                    FullName = model.Name,
                    Type = UserType.Admin,
                    Email = model.Email,

                };
                pLMDbContext.Users.Add(user);
                await pLMDbContext.SaveChangesAsync();


                identityResult = await userManager.AddToRolesAsync(identityUser, new string[] { "Admin" });
                if (identityResult.Succeeded)
                {
                    return Ok("Admin Adedd Successfully");
                }


            }
            else
            {
                return BadRequest("Not saved");
            }
            return BadRequest("something went wrong");
        }
        [HttpPost]
        [Route("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Password changed successfully.");
        }
        [HttpPost]
        [Route("SendEmail")]
        [Authorize]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailDTO model)
        {
            try
            {
                await emailSender.SendEmailAsync(model.toemail, model.subject, model.message);
                return Ok("Email sent Succefully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetUserById")]
        //[Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                // Retrieve the user from the database by their ID
                var user = await pLMDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

                // If the user is not found, return NotFound response
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Map the user to UserDTO
                var userDTO = new UserDTO
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Type = user.Type.ToString() // Convert enum to string
                };

                // Return the DTO object
                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                // Return an internal server error if something goes wrong
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
