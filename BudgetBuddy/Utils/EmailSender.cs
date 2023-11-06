using System.Net;
using System.Net.Mail;

namespace BudgetBuddy.Utils
{
    public class EmailSender
    {
        private static readonly string SmtpServer = "smtp.gmail.com";
        private static readonly int SmtpPort = 587;
        private static readonly string SenderEmail = "bdgtbuddy@gmail.com";
        private static readonly string SenderPassword = "gsdreqlffgxdeomd"; // TODO: Replace with your actual password or use an App Password
        private readonly ILogger<EmailSender> _logger;

        // Method to initialize the logger
        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static bool Send(string to, string subject, string messageBody)
        {
            var message = new MailMessage(SenderEmail, to)
            {
                Subject = subject,
                IsBodyHtml = true,
                Body = messageBody,
            };

            return SendEmail(message, to);
        }

        private static bool SendEmail(MailMessage message, string recipient)
        {
            using var client = new SmtpClient(SmtpServer, SmtpPort)
            {
                Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                EnableSsl = true,
            };

            try
            {
                client.Send(message);
                //_logger.LogInformation("Email sent successfully to: {Recipient}", recipient);
                return true;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error sending email to: {Recipient}", recipient);
                return false;
            }


        }


    }
}
