using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using KlodTattooWeb.Models;
using Resend;

namespace KlodTattooWeb.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        // Metodo standard per Identity
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await ExecuteSendEmailAsync(email, subject, htmlMessage, null);
        }

        // Metodo custom per BookingController con Reply-To
        public async Task SendContactEmailAsync(string clientEmail, string clientName, string subject, string htmlMessage)
        {
            await ExecuteSendEmailAsync(_emailSettings.SenderEmail, subject, htmlMessage, clientEmail);
        }

        private async Task ExecuteSendEmailAsync(string toEmail, string subject, string htmlMessage, string? replyToEmail)
        {
            try
            {
                _logger.LogInformation($"📧 [START] Inizio invio email a: {toEmail}");

                // Verifica configurazione
                if (string.IsNullOrEmpty(_emailSettings.ResendApiKey))
                {
                    _logger.LogError("❌ [ERROR] Resend API Key non configurata!");
                    throw new InvalidOperationException("Resend API Key mancante nelle configurazioni");
                }

                var resend = ResendClient.Create(_emailSettings.ResendApiKey);

                var message = new EmailMessage
                {
                    From = $"{_emailSettings.SenderName} <{_emailSettings.SenderEmail}>",
                    To = new[] { toEmail },
                    Subject = subject,
                    HtmlBody = htmlMessage
                };

                // Aggiungi Reply-To se presente (per booking requests)
                if (!string.IsNullOrEmpty(replyToEmail))
                {
                    message.ReplyTo = new[] { replyToEmail };
                    _logger.LogInformation($"📮 [REPLY-TO] Impostato reply-to: {replyToEmail}");
                }

                _logger.LogInformation($"🚀 [SEND] Invio tramite Resend API...");
                var response = await resend.EmailSendAsync(message);

                if (response != null && response.Success && response.Content != Guid.Empty)
                {
                    _logger.LogInformation($"✅ [SUCCESS] Email inviata con successo! ID: {response.Content}");
                }
                else
                {
                    _logger.LogWarning("⚠️ [WARNING] Email inviata ma nessun ID ricevuto");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [ERROR] ERRORE CRITICO EMAIL a {toEmail}: {ex.Message}");
                _logger.LogError($"❌ [STACK] {ex.StackTrace}");
                throw;
            }
        }
    }
}