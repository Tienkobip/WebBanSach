using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using WebApp.Shared.Dtos.Management.Auth;

namespace WebApp.Shared.Validations.Management.Auth
{
    public class AdminVerifyOtpDtoValidator : AbstractValidator<AdminVerifyOtpDto>
    {
        public AdminVerifyOtpDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Định dạng Email không hợp lệ");
            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("Mã OTP không được để trống")
                .Length(6).WithMessage("Mã OTP phải có 6 ký tự")
                .Matches("^[0-9]+$").WithMessage("Mã OTP chỉ được chứa các chữ số")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}
