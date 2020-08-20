using GoApi.Data.Constants;
using GoApi.Data.Models;
using GoApi.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GoApi.Services.Implementations
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        public MailService(MailSettings mailSettings)
        {
            _mailSettings = mailSettings;
        }

        public async Task SendConfirmationEmailContractorAsync(Organisation org, ApplicationUser user, string confirmationLink)
        {
            string text = Mail.ConfirmationContractorBody(user.UserName, org.OrganisationName, _mailSettings.SenderName, confirmationLink);
            string subject = Mail.ConfirmationSubject(_mailSettings.SenderName);
            await SendMailAsync(user.UserName, user.Email, subject, text);
        }

        public async Task SendMailAsync(string toName, string toAddress, string subject, string text)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.Email));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = text
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.Port);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
