using BusinessLayer.Helpers;
using BusinessLayer.IService;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BusinessLayer.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _emailSettings.SmtpHost,
                    _emailSettings.SmtpPort,
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                if (_emailSettings.Password == "your-gmail-app-password" || _emailSettings.SenderEmail == "your-email@gmail.com")
                {
                    _logger.LogWarning("Email sending failed but skipped because default placeholder credentials were used. Check console log for verification code.");
                    return;
                }
                throw new InvalidOperationException($"Failed to send email to {toEmail}. Please try again later.");
            }
        }

        public async Task SendVerificationEmailAsync(string toEmail, string fullName, string otpCode)
        {
            _logger.LogInformation("\n==================================================\n[EMAIL VERIFICATION OTP]\nTo: {Email} ({Name})\nOTP Code: {OtpCode}\n==================================================\n", toEmail, fullName, otpCode);

            var subject = "AutoWash Pro - Xác thực email đăng ký";
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
    <div style='max-width: 500px; margin: 0 auto; background: #ffffff; border-radius: 12px; padding: 40px; box-shadow: 0 2px 12px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 30px;'>
            <h1 style='color: #1a73e8; margin: 0;'>🚗 AutoWash Pro</h1>
            <p style='color: #666; margin-top: 5px;'>Hệ thống rửa xe thông minh</p>
        </div>
        <p style='color: #333;'>Xin chào <strong>{fullName}</strong>,</p>
        <p style='color: #555;'>Cảm ơn bạn đã đăng ký tài khoản. Vui lòng sử dụng mã OTP bên dưới để xác thực email:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <div style='display: inline-block; background: linear-gradient(135deg, #1a73e8, #0d47a1); color: #ffffff; font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 15px 40px; border-radius: 8px;'>
                {otpCode}
            </div>
        </div>
        <p style='color: #555; text-align: center;'>Mã OTP có hiệu lực trong <strong>15 phút</strong>.</p>
        <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
        <p style='color: #999; font-size: 12px; text-align: center;'>Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email này.</p>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetUrl) => SendEmailAsync(toEmail, "AutoWash Pro - Đặt lại mật khẩu", $"<p>Xin chào <strong>{System.Net.WebUtility.HtmlEncode(fullName)}</strong>,</p><p>Liên kết đặt lại mật khẩu có hiệu lực trong 15 phút:</p><p><a href=\"{System.Net.WebUtility.HtmlEncode(resetUrl)}\">Đặt lại mật khẩu</a></p><p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>");
    }
}
