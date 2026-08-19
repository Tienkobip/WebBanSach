using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Api.Services.Interfaces;
using WebApp.Shared.Dtos.Common;
using WebApp.Shared.Dtos.Management.Auth;

namespace WebApp.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAdminAuthService _adminAuthService;
        private readonly IValidator<AdminLoginDto> _loginValidator;
        private readonly IValidator<AdminForgotPasswordDto> _forgotPasswordValidator;
        private readonly IValidator<AdminResetPasswordDto> _resetPasswordValidator;
        private readonly IValidator<AdminVerifyOtpDto> _verifyOtpValidator;

        public AuthController(IAdminAuthService adminAuthService, IValidator<AdminLoginDto> loginValidator, IValidator<AdminForgotPasswordDto> forgotPasswordValidator, IValidator<AdminResetPasswordDto> resetPasswordValidator, IValidator<AdminVerifyOtpDto> verifyOtpValidator)
        {
            _adminAuthService = adminAuthService;
            _loginValidator = loginValidator;
            _forgotPasswordValidator = forgotPasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _verifyOtpValidator = verifyOtpValidator;
        }

        [HttpPost("admin-login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> LoginAdmin([FromBody] AdminLoginDto dto)
        {
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResult("Dữ liệu không hợp lệ", errors));
            }

            var response = await _adminAuthService.LoginAdminAsync(dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("admin-me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GetCurrentUser()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.FailureResult("Chưa đăng nhập."));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không tìm thấy thông tin người dùng.");
            }

            var response = await _adminAuthService.GetCurrentUserAsync(userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("admin-logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(ApiResponse<bool>.SuccessResult(true,"Đăng xuất thành công."));
        }

        [HttpPost("admin-forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] AdminForgotPasswordDto dto)
        {
            var validationResult = await _forgotPasswordValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.FailureResult("Dữ liệu không hợp lệ", errors));
            }

            var response = await _adminAuthService.ForgotPasswordAsync(dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("admin-reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] AdminResetPasswordDto dto)
        {
            var validationResult = await _resetPasswordValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.FailureResult("Dữ liệu không hợp lệ", errors));
            }

            var response = await _adminAuthService.ResetPasswordAsync(dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("admin-verify-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyOtp([FromBody] AdminVerifyOtpDto dto)
        {
            var validationResult = await _verifyOtpValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.FailureResult("Dữ liệu không hợp lệ", errors));
            }

            var response = await _adminAuthService.VerifyOtpAsync(dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
