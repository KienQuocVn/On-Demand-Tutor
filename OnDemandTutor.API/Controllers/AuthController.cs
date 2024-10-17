using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnDemandTutor.ModelViews.AuthModelViews;
using OnDemandTutor.Contract.Services.Interface;
using System;
using System.Threading.Tasks;
using OnDemandTutor.Services.Service;
using Microsoft.AspNetCore.Authorization;
using OnDemandTutor.Repositories.Entity;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Core.Base;

using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;


namespace OnDemandTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        // Inject the user service via constructor
        public AuthController(IUserService userService, TokenService tokenService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        // Login API
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelView model)
        {
            _logger.LogInformation("Received login request for username: {Username}", model.Username);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for login request.");
                return BadRequest(ModelState);
            }

            // Authenticate user
            Accounts account = await _userService.AuthenticateAsync(model);
            if (account == null)
            {
                _logger.LogWarning("Invalid credentials for username: {Username}", model.Username);
                return Unauthorized("Invalid credentials");
            }

            var token = await _tokenService.GenerateJwtTokenAsync(account.Id.ToString(), account.UserName);

            _logger.LogInformation("Login successful for user: {UserName}", account.UserName);

            return Ok(new
            {
                Token = token,
            });
        }

        // Register API
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateAccountModel model)
        {
            _logger.LogInformation("Received registration request for email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for registration request.");
                return BadRequest(ModelState);
            }

            // Create new account
            Accounts account = await _userService.CreateAccountAsync(model);
            if (account == null)
            {
                _logger.LogWarning("Failed to create account for email: {Email}", model.Email);
                return BadRequest("Failed to create account");
            }

            _logger.LogInformation("Account created successfully for email: {Email}", model.Email);

            return Ok("Success");
        }
        [HttpPut("update-account/{userId}")]
        public async Task<IActionResult> UpdateAccount(string userId, [FromBody] UpdateUserModel model)
        {
            _logger.LogInformation("Updating account for user: {UserId}", userId);

            var result = await _userService.UpdateUserAsync(userId, model);
            if (!result)
            {
                return NotFound("User not found or update failed.");
            }

            return Ok("Account updated successfully.");
        }

        [Authorize]
        [HttpPost("add-role-to-user")]
        public async Task<IActionResult> AddRoleToUser([FromBody] AddRoleModel model)
        {
            _logger.LogInformation("Received request to add role: {RoleName} to user: {UserId}", model.RoleName, model.UserId);

            var result = await _userService.AddRoleToAccountAsync(model.UserId, model.RoleName);
            if (!result)
            {
                _logger.LogWarning("Failed to add role: {RoleName} to user: {UserId}", model.RoleName, model.UserId);
                return BadRequest("Failed to add role to user or user/role does not exist.");
            }

            _logger.LogInformation("Role: {RoleName} added to user: {UserId} successfully", model.RoleName, model.UserId);

            return Ok("Role added to user successfully.");
        }

        [HttpPost("add-claim-to-role")]
        public async Task<IActionResult> AddClaimToRole([FromBody] AddClaimToRoleModel model)
        {
            _logger.LogInformation("Received request to add claim to role: {RoleId}", model.RoleId);

            var result = await _userService.AddClaimToRoleAsync(model.RoleId, model.ClaimType, model.ClaimValue, model.CreatedBy);
            if (!result)
            {
                _logger.LogWarning("Failed to add claim to role: {RoleId}", model.RoleId);
                return BadRequest("Failed to add claim to role or role does not exist.");
            }

            _logger.LogInformation("Claim added to role: {RoleId} successfully", model.RoleId);

            return Ok("Claim added to role successfully.");
        }

        [HttpPost("add-claim")]
        public async Task<IActionResult> AddClaimToUser([FromBody] AddClaimModel model)
        {
            _logger.LogInformation("Received request to add claim to user: {UserId}", model.UserId);

            var result = await _userService.AddClaimToUserAsync(model.UserId, model.ClaimType, model.ClaimValue, model.CreatedBy);
            if (!result)
            {
                _logger.LogWarning("Failed to add claim to user: {UserId}", model.UserId);
                return BadRequest("Failed to add claim.");
            }

            _logger.LogInformation("Claim added to user: {UserId} successfully", model.UserId);

            return Ok("Claim added successfully.");
        }

        [HttpGet("user-claims/{userId}")]
        public async Task<IActionResult> GetUserClaims(Guid userId)
        {
            _logger.LogInformation("Received request to get claims for user: {UserId}", userId);

            var claims = await _userService.GetUserClaimsAsync(userId);

            _logger.LogInformation("Retrieved claims for user: {UserId}", userId);

            return Ok(claims);
        }

        [HttpPut("update-claim")]
        public async Task<IActionResult> UpdateClaim([FromBody] UpdateClaimModel model)
        {
            _logger.LogInformation("Received request to update claim: {ClaimId}", model.ClaimId);

            var result = await _userService.UpdateClaimAsync(model.ClaimId, model.ClaimType, model.ClaimValue, model.UpdatedBy);
            if (!result)
            {
                _logger.LogWarning("Failed to update claim: {ClaimId}", model.ClaimId);
                return BadRequest("Failed to update claim.");
            }

            _logger.LogInformation("Claim: {ClaimId} updated successfully", model.ClaimId);

            return Ok("Claim updated successfully.");
        }

        [HttpDelete("delete-claim/{claimId}")]
        public async Task<IActionResult> SoftDeleteClaim(Guid claimId, [FromBody] string deletedBy)
        {
            _logger.LogInformation("Received request to soft delete claim: {ClaimId}", claimId);

            var result = await _userService.SoftDeleteClaimAsync(claimId, deletedBy);
            if (!result)
            {
                _logger.LogWarning("Failed to soft delete claim: {ClaimId}", claimId);
                return BadRequest("Failed to delete claim.");
            }

            _logger.LogInformation("Claim: {ClaimId} soft deleted successfully", claimId);

            return Ok("Claim deleted successfully.");
        }

        [HttpGet("getAccountByRolename")]
        public async Task<ActionResult<BasePaginatedList<Schedule>>> GetAllSchedules(int pageNumber = 1, int pageSize = 5, string roleName = null)
        {
            try
            {
                var result = await _userService.GetAccountsByRoleAsync(roleName, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        //tạo mặc định
        [HttpPost("create-default-roles")]
        public async Task<IActionResult> CreateDefaultRoles(string createdBy)
        {
            if (string.IsNullOrEmpty(createdBy))
            {
                return BadRequest("Created by information is required.");
            }

            // Các role mặc định
            var roles = new[] { "user", "moderator", "admin", "student", "tutor" };

            foreach (var role in roles)
            {
                var result = await _userService.AddRoleAsync(role, createdBy);

                if (!result)
                {
                    return BadRequest($"Role '{role}' already exists.");
                }
            }

            return Ok("Default roles created successfully.");
        }
        //[HttpGet("signin-google")]
        //public IActionResult SignInWithGoogle()
        //{
        //    var redirectUrl = Url.Action("GoogleResponse", "Auth");
        //    var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        //    return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        //}

        //[HttpGet("google-response")]
        //public async Task<IActionResult> GoogleResponse()
        //{
        //    var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        //    if (!result.Succeeded)
        //        return BadRequest(); // Xử lý lỗi  

        //    // Lấy thông tin người dùng từ result  
        //    var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        //    // Xử lý đăng nhập (tạo token, lưu vào database, v.v.)  

        //    return Ok(new { Email = email });
        //}

        //[HttpGet("signin-facebook")]
        //public IActionResult SignInWithFacebook()
        //{
        //    var redirectUrl = Url.Action("FacebookResponse", "Auth");
        //    var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        //    return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        //}

        //[HttpGet("facebook-response")]
        //public async Task<IActionResult> FacebookResponse()
        //{
        //    var result = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);
        //    if (!result.Succeeded)
        //        return BadRequest(); // Xử lý lỗi  

        //    // Lấy thông tin người dùng từ result  
        //    var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        //    // Xử lý đăng nhập (tạo token, lưu vào cơ sở dữ liệu, v.v.)  

        //    return Ok(new { Email = email });
        //}
    }
}
