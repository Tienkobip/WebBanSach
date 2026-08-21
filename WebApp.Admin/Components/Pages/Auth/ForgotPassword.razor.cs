using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using WebApp.Shared.Dtos.Customer.Auth;
using WebApp.Shared.Dtos.Management.Auth;

namespace WebApp.Admin.Components.Pages.Auth
{
    [AllowAnonymous]
    public partial class ForgotPassword
    {
        private AdminResetPasswordDto forgotPasswordModel = new();
        private EditContext? editContext;
        private int currentStep = 1;
        private bool isOtpSent = false;
        private bool isTimerRunning = false;
        private bool isSubmitting = false;
        private bool showNewPassword = false;
        private bool showConfirmPassword = false;
        private int countdownSeconds = 60;
        private string? errorMessage;

        private List<string>? validationErrors;

        protected override void OnInitialized()
        {
            editContext = new EditContext(forgotPasswordModel);
        }

        // Điều kiện Validate cho các nút
        private bool CanSendOtp => !string.IsNullOrWhiteSpace(forgotPasswordModel.Email) && !isTimerRunning && !isOtpSent;
        private bool CanVerifyOtp => isOtpSent && forgotPasswordModel.OtpCode?.Trim().Length == 6;
        private bool CanResetPassword => !string.IsNullOrWhiteSpace(forgotPasswordModel.NewPassword)
                                       && forgotPasswordModel.NewPassword == forgotPasswordModel.ConfirmNewPassword
                                       && forgotPasswordModel.NewPassword.Length >= 6;

        //private bool CanSendOtp => !string.IsNullOrWhiteSpace(forgotPasswordModel.Email) && !isTimerRunning && !isSubmitting;
        //private bool CanVerifyOtp => isOtpSent && !isSubmitting;
        //private bool CanResetPassword => !string.IsNullOrWhiteSpace(forgotPasswordModel.NewPassword)
        //                       && !string.IsNullOrWhiteSpace(forgotPasswordModel.ConfirmNewPassword)
        //                       && !isSubmitting;

        private void ToggleShowNewPassword() => showNewPassword = !showNewPassword;
        private void ToggleShowConfirmPassword() => showConfirmPassword = !showConfirmPassword;

        private async Task HandleSendOtp()
        {
            if (isSubmitting || isTimerRunning) return;
            var fieldIdentifier = new FieldIdentifier(forgotPasswordModel, nameof(forgotPasswordModel.Email));
            editContext?.NotifyFieldChanged(fieldIdentifier);

            if (editContext?.GetValidationMessages(fieldIdentifier).Any() == true)
            {
                errorMessage = "Vui lòng nhập email hợp lệ.";
                return;
            }

            isSubmitting = true;
            errorMessage = null;

            try
            {
                var result = await AuthClientService.ForgotPasswordAsync(new AdminForgotPasswordDto(forgotPasswordModel.Email));
                if (result.Success)
                {
                    isOtpSent = true;
                    _ = StartTimer();
                }
                else
                {
                    errorMessage = result.Message ?? "Gửi OTP thất bại.";
                }
            }
            catch (Exception)
            {
                errorMessage = "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.";
            }
            finally
            {
                isSubmitting = false;
            }
        }

        private async Task HandleVerifyOtp()
        {
            var fieldIdentifier = new FieldIdentifier(forgotPasswordModel, nameof(forgotPasswordModel.OtpCode));
            editContext?.NotifyFieldChanged(fieldIdentifier);

            if (editContext?.GetValidationMessages(fieldIdentifier).Any() == true)
            {
                errorMessage = "Vui lòng nhập mã OTP 6 số hợp lệ.";
                return;
            }

            isSubmitting = true;
            errorMessage = null;

            try
            {
                var result = await AuthClientService.VerifyOtpAsync(new AdminVerifyOtpDto(forgotPasswordModel.Email, forgotPasswordModel.OtpCode));
                if (result.Success)
                {
                    currentStep = 2; // Chuyển bước thành công
                }
                else
                {
                    errorMessage = result.Message ?? "Mã OTP không chính xác hoặc đã hết hạn.";
                }
            }
            catch (Exception)
            {
                errorMessage = "Lỗi kết nối máy chủ khi xác thực OTP.";
            }
            finally
            {
                isSubmitting = false;
            }
        }

        private async Task HandleResetPassword()
        {
            if (editContext?.Validate() == false)
            {
                errorMessage = "Vui lòng điền đầy đủ thông tin hợp lệ.";
                return;
            }

            isSubmitting = true;
            errorMessage = null;
            validationErrors = null;

            // TODO: Gọi API Reset Password (AdminResetPasswordDto)
            try
            {
                var result = await AuthClientService.ResetPasswordAsync(forgotPasswordModel);
                if (result.Success)
                {
                    Navigation.NavigateTo("/management/login");
                }
                else
                {
                    // Hứng thông báo lỗi chung và danh sách lỗi chi tiết từ API
                    errorMessage = result.Message ?? "Đặt lại mật khẩu thất bại.";
                    validationErrors = result.Errors;
                }
            }
            catch (Exception)
            {
                errorMessage = "Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại kết nối mạng.";
            }
            finally
            {
                isSubmitting = false; // Trả lại trạng thái cho UI
            }
        }

        private async Task StartTimer()
        {
            isTimerRunning = true;
            countdownSeconds = 60;
            while (countdownSeconds > 0)
            {
                await Task.Delay(1000);
                countdownSeconds--;
                StateHasChanged();
            }
            isTimerRunning = false;
        }
    }
}
