using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SendGmail
    {
        MailSetting _mailSet { get; set; }
        public SendGmail(IOptions<MailSetting> mailSetting)
        {
            _mailSet = mailSetting.Value;
        }
        public async Task<string> SendMail(MailContent mailContent)
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress(_mailSet.Name, _mailSet.Mail);
            email.From.Add(new MailboxAddress(_mailSet.Name, _mailSet.Mail));
            email.To.Add(new MailboxAddress(mailContent.To, mailContent.To));
            email.Subject = mailContent.Sub;
            var builder = new BodyBuilder();
            builder.HtmlBody = mailContent.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await smtp.ConnectAsync(_mailSet.Host, _mailSet.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSet.Mail, _mailSet.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                return ("loi" + ex.ToString());
            }
            smtp.Disconnect(true);
            return "";
        }
    }
    public class MailContent
    {
        public string? To { get; set; }
        public string? Sub { get; set; }
        public string? Body { get; set; }
    }
}
