using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Api.Services.Interfaces;
using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.User;

namespace WebApp.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;

        public UserController(IUserService userService, IValidator<UpdateProfileDto> updateProfileValidator, IValidator<ChangePasswordDto> changePasswordValidator)
        {
            _userService = userService;
            _updateProfileValidator = updateProfileValidator;
            _changePasswordValidator = changePasswordValidator;
        }

        // GET: api/user/profile
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<UserProfileDto>.FailureResult("Phiên đăng nhập đã hết hạn hoặc không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại."));
            }   

            var response = await _userService.GetUserProfileAsync(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        // PUT: api/user/profile
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var validationResult = await _updateProfileValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.FailureResult("Dữ liệu đầu vào không hợp lệ.", errors));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.FailureResult("Phiên đăng nhập đã hết hạn hoặc không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại."));
            }

            var response = await _userService.UpdateUserProfileAsync(userId, dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        // POST: api/user/change-password
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var validationResult = await _changePasswordValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.FailureResult("Dữ liệu đầu vào không hợp lệ.", errors));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.FailureResult("Phiên đăng nhập đã hết hạn hoặc không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại."));
            }

            var response = await _userService.ChangePasswordAsync(userId, dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
