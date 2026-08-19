using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Shared.Dtos.Management.Auth;

namespace WebApp.Shared.Validations.Management.Auth
{
    public class AdminForgotPasswordDtoValidator : AbstractValidator<AdminForgotPasswordDto>
    {
        public AdminForgotPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Định dạng Email không hợp lệ");
        }
    }
}
