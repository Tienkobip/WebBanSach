namespace WebApp.Api.Utilities
{
    public static class EmailTemplates
    {
        public static string GetOtpTemplate(string otpCode)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            </head>
            <body style='font-family: Arial, sans-serif; background-color: #F4F4F4; margin: 0; padding: 20px;'>
                <table align='center' border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 480px; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08);'>
                    <!-- Header Red Fahasa -->
                    <tr>
                        <td align='center' style='background-color: #C92127; padding: 20px; color: #ffffff;'>
                            <h2 style='margin: 0; font-size: 18px; font-weight: bold; letter-spacing: 1px; text-transform: uppercase;'>XÁC THỰC MÃ OTP</h2>
                        </td>
                    </tr>
                    <!-- Content Body -->
                    <tr>
                        <td style='padding: 30px 20px; text-align: center; color: #333333;'>
                            <p style='font-size: 14px; margin-bottom: 20px; color: #555555;'>Mã OTP dùng để xác minh đặt lại mật khẩu của bạn là:</p>
                            
                            <!-- Khung OTP -->
                            <div style='margin: 20px 0;'>
                                <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #C92127; background-color: #FFF0F0; padding: 12px 24px; border-radius: 6px; border: 1px dashed #C92127; display: inline-block;'>
                                    {otpCode}
                                </span>
                            </div>
                            
                            <p style='font-size: 12px; color: #888888; margin-top: 20px; line-height: 1.4;'>Mã OTP có hiệu lực trong <b>5 phút</b>.<br/>Vì lý do bảo mật, vui lòng không chia sẻ mã này cho người khác.</p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td align='center' style='background-color: #F8F8F8; padding: 15px; border-top: 1px solid #EEEEEE;'>
                            <p style='font-size: 13px; font-weight: bold; color: #C92127; margin: 0 0 4px 0; text-transform: uppercase;'>NHÀ SÁCH VĂN HÓA TRUYỀN THỐNG</p>
                            <p style='font-size: 11px; color: #888888; margin: 0;'>© {DateTime.Now.Year} All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }
    }
}