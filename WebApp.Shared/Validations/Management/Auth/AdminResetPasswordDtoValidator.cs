using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Shared.Dtos.Management.Auth;

namespace WebApp.Shared.Validations.Management.Auth
{
    public class AdminResetPasswordDtoValidator : AbstractValidator<AdminResetPasswordDto>
    {
        public AdminResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Định dạng Email không hợp lệ");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("Mã OTP không được để trống")
                .Length(6).WithMessage("Mã OTP phải có 6 ký tự")
                .Matches("^[0-9]+$").WithMessage("Mã OTP chỉ được chứa các chữ số")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu mới không được để trống")
                .MinimumLength(6).WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Vui lòng xác nhận mật khẩu mới")
                .Equal(x => x.NewPassword).WithMessage("Mật khẩu xác nhận không khớp");
        }
    }
}
