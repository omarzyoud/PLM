using System.Net.Mail;
using System.Net;

namespace PLM.api.Repositories
{
    public class CustomEmailSender: ICustomEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mail = "vPal.wiki@outlook.com";
            var pw = "vPal1234";
            var client = new SmtpClient("smtp-mail.outlook.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw)
            };
            var mailMessage = new MailMessage(
               from: mail,
               to: email,
               subject: subject,
               body: message
           );

            await client.SendMailAsync(mailMessage);
        }
    }
}
