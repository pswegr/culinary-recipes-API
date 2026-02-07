using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace CulinaryRecipes.API.Services
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

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
                {
                    Port = _emailSettings.SmtpPort,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword),
                    EnableSsl = true,
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SmtpHost, _emailSettings.SmtpUser),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex) when (IsBasicAuthDisabled(ex))
            {
                _logger.LogError(
                    ex,
                    "SMTP authentication failed for {SmtpServer}:{SmtpPort}. Basic authentication appears disabled for mailbox {SmtpUser}.",
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    _emailSettings.SmtpUser);

                throw new InvalidOperationException(
                    "SMTP authentication failed because basic authentication is disabled by the mail provider. Configure OAuth2 SMTP or switch to a provider/auth method that supports username/password.",
                    ex);
            }
        }

        private static bool IsBasicAuthDisabled(SmtpException ex)
        {
            return ex.Message.Contains("5.7.139", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("5.7.57", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("basic authentication is disabled", StringComparison.OrdinalIgnoreCase);
        }
    }
}
