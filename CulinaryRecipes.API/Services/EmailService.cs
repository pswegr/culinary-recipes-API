using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace CulinaryRecipes.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly SmtpClient _smtpClient;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            SmtpClient smtpClient,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _smtpClient = smtpClient;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var fromEmail = ResolveFromEmail();
                var fromName = string.IsNullOrWhiteSpace(_emailSettings.FromName)
                    ? "Culinary Recipes"
                    : _emailSettings.FromName;

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                await _smtpClient.SendMailAsync(mailMessage);
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

        private string ResolveFromEmail()
        {
            if (!string.IsNullOrWhiteSpace(_emailSettings.FromEmail))
            {
                return _emailSettings.FromEmail;
            }

            if (!string.IsNullOrWhiteSpace(_emailSettings.SmtpHost))
            {
                return _emailSettings.SmtpHost;
            }

            return _emailSettings.SmtpUser;
        }
    }
}
