using FluentValidation;
using WebApp.Shared.Dtos.User;

namespace WebApp.Api.Validations.User
{
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        private const int MaxBase64ImageLength = 7 * 1024 * 1024;

        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên không được để trống")
                .MaximumLength(150).WithMessage("Họ tên tối đa 150 ký tự");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today).WithMessage("Ngày sinh không hợp lệ")
                .When(x => x.DateOfBirth.HasValue);

            // Validation kiểm tra dung lượng Avatar upload (Max 5MB)
            When(x => !string.IsNullOrEmpty(x.AvatarBase64), () =>
            {
                RuleFor(x => x.AvatarBase64)
                    .Must(base64 => base64!.Length <= MaxBase64ImageLength)
                    .WithMessage("Kích thước ảnh đại diện không được vượt quá 5MB");
            });
        }
    }
}
