using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using KlodTattooWeb.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace KlodTattooWeb.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        // Metodo standard per Identity (Reset Password, Conferma Email)
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await ExecuteSendEmailAsync(email, subject, htmlMessage, null);
        }

        // NUOVO METODO: Da usare nel BookingController per le mail dai clienti
        // Permette di impostare il "Rispondi a" (Reply-To) verso il cliente
        public async Task SendContactEmailAsync(string clientEmail, string clientName, string subject, string htmlMessage)
        {
            // Inviamo la mail a NOI STESSI (SenderEmail), ma se clicchiamo rispondi, rispondiamo al CLIENTE (clientEmail)
            await ExecuteSendEmailAsync(_emailSettings.SenderEmail, subject, htmlMessage, clientEmail);
        }

        // Metodo privato che esegue l'invio reale
        private async Task ExecuteSendEmailAsync(string toEmail, string subject, string htmlMessage, string? replyToEmail)
        {
            var message = new MimeMessage();

            // Il mittente DEVE essere la mail autenticata (Tu)
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));

            // Il destinatario
            message.To.Add(new MailboxAddress("", toEmail));

            // Se c'è una mail per la risposta (quella del cliente), la aggiungiamo
            if (!string.IsNullOrEmpty(replyToEmail))
            {
                message.ReplyTo.Add(new MailboxAddress("", replyToEmail));
            }

            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            using var client = new SmtpClient();

            try
            {
                // Connessione a Google
                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    SecureSocketOptions.StartTls
                );

                // Login
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                // Invio
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                // In fase di sviluppo è utile vedere l'errore nella console
                Console.WriteLine($"Errore invio email: {ex.Message}");
                throw; // Rilanciamo l'errore per gestirlo nel Controller o mostrarlo all'utente
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}