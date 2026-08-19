using System;
using System.Collections.Generic;
using System.Text;

namespace WebApp.Shared.Dtos.Management.Auth
{
    public record AdminVerifyOtpDto
    (
        string Email,
        string OtpCode
    );
}
