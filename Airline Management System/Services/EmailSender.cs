using System.Net;
using System.Net.Mail;

namespace Airline_Management_System.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            string mailServer = emailSettings["MailServer"];
            int mailPort = int.Parse(emailSettings["MailPort"]);
            string senderName = emailSettings["SenderName"];
            string senderEmail = emailSettings["SenderEmail"];
            string password = emailSettings["Password"];

            var client = new SmtpClient(mailServer, mailPort)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true // Allows us to use <b>, <br>, etc.
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
