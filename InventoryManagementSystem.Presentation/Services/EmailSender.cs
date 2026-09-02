using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Options;

namespace InventoryManagementSystem.Presentation.Services
{
    public class EmailSender
    {
        private readonly EmailSettings _settings;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(
            IOptions<EmailSettings> settings,
            IWebHostEnvironment environment,
            ILogger<EmailSender> logger)
        {
            _settings = settings.Value;
            _environment = environment;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                _logger.LogWarning("No alert recipient is configured, so no mail was sent.");
                return;
            }

            if (!_settings.IsConfigured)
            {
                await SaveToDropFolderAsync(to, subject, htmlBody);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(
                    string.IsNullOrWhiteSpace(_settings.FromAddress) ? _settings.UserName : _settings.FromAddress,
                    _settings.FromName),
                Subject = subject,
                Body = textBody ?? StripTags(htmlBody),
                IsBodyHtml = false
            };

            message.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html));

            message.To.Add(to);

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(
                    _settings.UserName,
                    _settings.Password.Replace(" ", string.Empty))
            };

            try
            {
                await client.SendMailAsync(message);

                _logger.LogInformation(
                    "SMTP accepted the alert. From={From} To={To} Host={Host}:{Port}",
                    message.From!.Address,
                    string.Join(", ", message.To.Select(a => a.Address)),
                    _settings.Host,
                    _settings.Port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not send the expiry alert to {Recipient}.", to);

                await SaveToDropFolderAsync(to, subject, htmlBody);
            }
        }

        private static string StripTags(string html)
        {
            var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");

            text = WebUtility.HtmlDecode(text);

            return System.Text.RegularExpressions.Regex.Replace(text, @"\s{2,}", " ").Trim();
        }

        private async Task SaveToDropFolderAsync(string to, string subject, string htmlBody)
        {
            var folder = Path.Combine(_environment.ContentRootPath, _settings.DropFolder);

            Directory.CreateDirectory(folder);

            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var path = Path.Combine(folder, $"{stamp}.html");

            var document =
                $"<!doctype html><meta charset=\"utf-8\">" +
                $"<p><b>To:</b> {WebUtility.HtmlEncode(to)}<br>" +
                $"<b>Subject:</b> {WebUtility.HtmlEncode(subject)}<br>" +
                $"<b>Written:</b> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p><hr>{htmlBody}";

            await File.WriteAllTextAsync(path, document);

            _logger.LogInformation("SMTP is not configured, so the alert was written to {Path}.", path);
        }
    }
}
