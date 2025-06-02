// MyanvieBE/Services/EmailService.cs
using MailKit.Net.Smtp;
using MailKit.Security; // Cần cho SecureSocketOptions
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;

namespace MyanvieBE.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            _logger.LogInformation("Attempting to send email to {ToEmail} with subject: {Subject} using REAL SMTP (Gmail)", toEmail, subject);

            var mailSettings = _configuration.GetSection("MailSettings");
            var senderEmail = mailSettings["SenderEmail"];
            var senderName = mailSettings["SenderName"];
            var smtpHost = mailSettings["SmtpHost"];
            var smtpPortString = mailSettings["SmtpPort"];
            var smtpUser = mailSettings["SmtpUser"]; // <<< Sẽ đọc từ appsettings
            var smtpPass = mailSettings["SmtpPass"]; // <<< Sẽ đọc từ appsettings

            // Kiểm tra cấu hình
            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderName) ||
                string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPortString) ||
                string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass)) // Kiểm tra cả SmtpUser và SmtpPass
            {
                _logger.LogError("Real Email Send: Mail settings (SenderEmail, SenderName, SmtpHost, SmtpPort, SmtpUser, SmtpPass) are not fully configured.");
                return;
            }
            // ... (parse smtpPort, tạo MimeMessage như cũ) ...
            if (!int.TryParse(smtpPortString, out int smtpPort))
            {
                _logger.LogError("SmtpPort '{SmtpPortString}' is not a valid integer.", smtpPortString);
                return;
            }

            var emailMessage = new MimeMessage();
            try
            {
                emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));
                emailMessage.To.Add(MailboxAddress.Parse(toEmail));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };
                _logger.LogInformation("MimeMessage created for {ToEmail}.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create MimeMessage for recipient {ToEmail}.", toEmail);
                return;
            }

            using var smtpClient = new SmtpClient();
            try
            {
                _logger.LogInformation("Connecting to SMTP host {SmtpHost} on port {SmtpPort} using StartTls...", smtpHost, smtpPort);
                await smtpClient.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls); // Quan trọng cho Gmail port 587

                _logger.LogInformation("Authenticating with SMTP user {SmtpUser}...", smtpUser);
                await smtpClient.AuthenticateAsync(smtpUser, smtpPass); // Xác thực với Gmail

                _logger.LogInformation("Sending email to {ToEmail}...", toEmail);
                await smtpClient.SendAsync(emailMessage);
                _logger.LogInformation("Email sent successfully to {ToEmail} via Gmail.", toEmail);
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError(authEx, "SMTP Authentication Failed for user {SmtpUser}. Check App Password and Gmail account 2FA/App Password settings.", smtpUser);
            }
            catch (SmtpCommandException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP Command Exception while sending email to {ToEmail}. Status Code: {StatusCode}, Message: {ErrorMessage}",
                   toEmail, smtpEx.StatusCode, smtpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic SMTP failure while sending email to {ToEmail}. Details: {ExceptionMessage}", toEmail, ex.Message);
            }
            finally
            {
                if (smtpClient.IsConnected)
                {
                    await smtpClient.DisconnectAsync(true);
                    _logger.LogInformation("Disconnected from SMTP host.");
                }
            }
        }
    }
}