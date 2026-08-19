using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebApp.Api.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
