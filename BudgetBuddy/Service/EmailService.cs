using System.Net.Mail;
using System.Net;

namespace BudgetBuddy.Service
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string callbackUrl);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromAddress; // This should be your email from which you're sending emails

        public EmailService(string smtpHost, int smtpPort, string smtpUser, string smtpPass, string fromAddress)
        {
            _smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
            };
            _fromAddress = fromAddress;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string callbackUrl)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress),
                Subject = "Reset Password",
                Body = $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            try
            {
                await _smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Handle the exception, log it, etc.
                throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
            }
        }
    }

}
