using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CulinaryRecipes.API.Services
{
    public class EmailService : IEmailService
    {
        private const string DefaultMailjetApiUrl = "https://api.mailjet.com/v3.1/send";

        private readonly EmailSettings _emailSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            HttpClient httpClient,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var apiKey = ResolveApiKey();
            var apiSecret = ResolveApiSecret();
            var fromEmail = ResolveFromEmail();
            var fromName = ResolveFromName();
            var mailjetApiUrl = ResolveMailjetApiUrl();

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException(
                    "Mailjet credentials are missing. Configure SMTP:ApiKey and SMTP:ApiSecret or set MJ_APIKEY_PUBLIC and MJ_APIKEY_PRIVATE environment variables.");
            }

            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException(
                    "Mailjet sender email is missing. Configure SMTP:FromEmail with a verified sender/domain in Mailjet.");
            }

            var payload = new
            {
                Messages = new[]
                {
                    new
                    {
                        From = new
                        {
                            Email = fromEmail,
                            Name = fromName,
                        },
                        To = new[]
                        {
                            new
                            {
                                Email = toEmail,
                            },
                        },
                        Subject = subject,
                        TextPart = BuildTextPart(message),
                        HTMLPart = message,
                    },
                },
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, mailjetApiUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            };

            request.Headers.Authorization = CreateBasicAuthHeader(apiKey, apiSecret);

            try
            {
                using var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Mailjet send failed with status {StatusCode}. Response: {ResponseBody}",
                        (int)response.StatusCode,
                        responseBody);

                    if (IsSenderValidationError(responseBody))
                    {
                        throw new InvalidOperationException(
                            "Mailjet rejected the sender address. Verify SMTP:FromEmail is active in the same Mailjet account/subaccount as your API key.");
                    }

                    throw new InvalidOperationException(
                        $"Mailjet send failed with status {(int)response.StatusCode}. Check API key/secret and sender/domain verification.");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Mailjet API request failed for {MailjetApiUrl}.", mailjetApiUrl);
                throw new InvalidOperationException(
                    "Mailjet API request failed. Check outbound network access and Mailjet endpoint configuration.",
                    ex);
            }
        }

        private string ResolveApiKey()
        {
            return FirstNonEmpty(
                _emailSettings.ApiKey,
                _emailSettings.SmtpUser,
                Environment.GetEnvironmentVariable("MJ_APIKEY_PUBLIC"));
        }

        private string ResolveApiSecret()
        {
            return FirstNonEmpty(
                _emailSettings.ApiSecret,
                _emailSettings.SmtpPassword,
                Environment.GetEnvironmentVariable("MJ_APIKEY_PRIVATE"));
        }

        private string ResolveFromEmail()
        {
            return FirstNonEmpty(
                _emailSettings.FromEmail,
                _emailSettings.SmtpHost,
                Environment.GetEnvironmentVariable("MAILJET_FROM_EMAIL"),
                _emailSettings.SmtpUser);
        }

        private string ResolveFromName()
        {
            return string.IsNullOrWhiteSpace(_emailSettings.FromName)
                ? "Culinary Recipes"
                : _emailSettings.FromName;
        }

        private string ResolveMailjetApiUrl()
        {
            if (!string.IsNullOrWhiteSpace(_emailSettings.MailjetApiUrl))
            {
                return _emailSettings.MailjetApiUrl;
            }

            if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer))
            {
                return DefaultMailjetApiUrl;
            }

            var host = _emailSettings.SmtpServer.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? _emailSettings.SmtpServer
                : $"https://{_emailSettings.SmtpServer}";

            return $"{host.TrimEnd('/')}/v3.1/send";
        }

        private static AuthenticationHeaderValue CreateBasicAuthHeader(string username, string password)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            return new AuthenticationHeaderValue("Basic", token);
        }

        private static string BuildTextPart(string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(htmlMessage))
            {
                return string.Empty;
            }

            var noTags = Regex.Replace(htmlMessage, "<.*?>", string.Empty);
            return System.Net.WebUtility.HtmlDecode(noTags).Trim();
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static bool IsSenderValidationError(string responseBody)
        {
            return responseBody.Contains("sender", StringComparison.OrdinalIgnoreCase)
                && responseBody.Contains("validat", StringComparison.OrdinalIgnoreCase);
        }
    }
}
