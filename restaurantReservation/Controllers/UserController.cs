using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using restaurantReservation.Models;
using restaurantReservation.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace restaurantReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly IRepository _repository;
        private readonly IUserClaimsPrincipalFactory<User> _claimsPrincipalFactory;
        private readonly IConfiguration _configuration;

        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<User> userManager, IUserClaimsPrincipalFactory<User> claimsPrincipalFactory, IConfiguration configuration, IRepository repository, ILogger<UserController> logger, IAuthenticationService authenticationService)
        {
            _userManager = userManager;
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _configuration = configuration;
            _repository = repository;
            _logger = logger;
            _authenticationService = authenticationService;
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> DeleteUserByEmailAsync(string userEmail)
        {
            try
            {
                var deleted = await _repository.DeleteUserAsync(userEmail, _userManager);

                if (deleted)
                {
                    return Ok();
                }
                else
                {
                    return NotFound("User not found.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(EditPasswordViewModel uvm)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(uvm.EmailAddress);

                if (user == null)
                {
                    // User not found
                    return NotFound("User not found.");
                }

                // Generate a password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Reset the user's password
                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, uvm.NewPassword);

                if (!resetPasswordResult.Succeeded)
                {
                    // Failed to reset password
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to reset password. Please try again.");
                }

                return Ok(new { message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }



        [HttpPut]
        [Route("EditProfile")]
        public async Task<IActionResult> EditProfile(EditUserProfileViewModel uvm)
        {
            try
            {
                // Retrieve the user based on the provided user ID or email
                var user = await _userManager.FindByEmailAsync(uvm.emailaddress);

                if (user == null)
                {
                    // User not found
                    return NotFound("User not found.");
                }

                // Update the user properties
                user.FullName = uvm.FirstName;
                user.Surname = uvm.LastName;
                user.Email = uvm.newEmailaddress; // Updated email address
                user.PhoneNumber = uvm.phone;
                user.Address = uvm.address;

                // Validate if the new email address is unique
                var existingUserWithEmail = await _userManager.FindByEmailAsync(uvm.newEmailaddress);
                if (existingUserWithEmail != null && existingUserWithEmail.Id != user.Id)
                {
                    // Email address is already in use by another user
                    return BadRequest("Email address is already in use.");
                }

                // Update the user
                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    return Ok("Profile updated successfully.");
                }
                else
                {
                    var errors = updateResult.Errors.Select(e => e.Description);
                    return BadRequest(errors);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }



        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterViewModel uvm)
        {
            var user = await _userManager.FindByIdAsync(uvm.emailaddress);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FullName = uvm.FirstName,
                    Surname = uvm.LastName,
                    UserName = uvm.emailaddress,
                    Email = uvm.emailaddress,
                    PhoneNumber = uvm.phone,
                    Address = uvm.address,
                };

                var result = await _userManager.CreateAsync(user, uvm.password);

                if (result.Errors.Count() > 0) return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error. Please contact support.");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

            }
            else
            {
                return Forbid("Account already exists.");
            }

            return Ok();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> Login(LoginViewModel uvm)
        {
            var user = await _userManager.FindByNameAsync(uvm.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, uvm.Password)
)
            {
                try
                {
                    var userDetails = await GetUserDetails(user);

                    var token = GenerateJWTToken(user);

                    var response = new
                    {
                        Token = token,
                        User = userDetails
                    };
                    return Ok(response);

                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error. Please contact support.");
                }
            }
            else
            {
                return NotFound("Does not exist");
            }

        }

        [HttpPost]
        [Route("EditPassword")]
        public async Task<IActionResult> EditPassword(EditPasswordViewModel model)
        {
            // Retrieve the user based on the provided user ID or email
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);

            if (user == null)
            {
                // User not found
                return NotFound("User not found.");
            }

            // Verify the current password
            var passwordCheckResult = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheckResult)
            {
                // Current password does not match
                return BadRequest("Invalid current password.");
            }

            // Update the password
            var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!passwordChangeResult.Succeeded)
            {
                // Failed to update the password
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update password. Please try again.");
            }

            // Password successfully updated
            return Ok(new { message = "Password updated successfully." });
        }



        private async Task<RegisterViewModel> GetUserDetails(User user)
        {

            var userDetails = new RegisterViewModel
            {
                FirstName = user.FullName,
                LastName = user.Surname,
                emailaddress = user.Email,
                phone = user.PhoneNumber,
                address = user.Address

            };

            return userDetails;
        }

        [HttpGet]
        private ActionResult GenerateJWTToken(User user)
        {
            // Create JWT Token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Tokens:Issuer"],
                _configuration["Tokens:Audience"],
                claims,
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddHours(3)
            );

            return Created("", new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = user.UserName
            });
        }
    }
}
