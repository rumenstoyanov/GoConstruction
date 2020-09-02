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

        public Dictionary<string, string> GetNameAddressPairs(List<ApplicationUser> recepients)
        {
            var outDict = new Dictionary<string, string>();
            foreach (var r in recepients)
            {
                outDict.Add(r.FullName, r.Email);
            }
            return outDict;

        }

        public async Task SendConfirmationEmailAndPasswordNonContractorAsync(Organisation org, ApplicationUser user, ApplicationUser inviter, string seniority, string confirmationLink, string password)
        {
            string text = Mail.ConfirmationAndPasswordNonContractorBody(user.FullName, org.OrganisationName, _mailSettings.SenderName, confirmationLink, seniority, password, inviter.FullName);
            string subject = Mail.ConfirmationSubject(_mailSettings.SenderName);
            await SendMailAsync(user.FullName, user.Email, subject, text);
        }

        public async Task SendConfirmationEmailContractorAsync(Organisation org, ApplicationUser user, string confirmationLink)
        {
            string text = Mail.ConfirmationContractorBody(user.FullName, org.OrganisationName, _mailSettings.SenderName, confirmationLink);
            string subject = Mail.ConfirmationSubject(_mailSettings.SenderName);
            await SendMailAsync(user.FullName, user.Email, subject, text);
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

        public async Task SendMailAsync(Dictionary<string, string> nameAddressPairs, string subject, string text)
        {
            var emailTasks = new List<Task>();
            foreach (var name in nameAddressPairs.Keys)
            {
                emailTasks.Add(SendMailAsync(name, nameAddressPairs[name], subject, text));
            }
            await Task.WhenAll(emailTasks);
        }

        public async Task SendResetPasswordEmailAsync(ApplicationUser user, string newPassword)
        {
            string text = Mail.ResetPasswordBody(user.FullName, _mailSettings.SenderName, newPassword);
            string subject = Mail.ResetPasswordSubject(_mailSettings.SenderName);
            await SendMailAsync(user.FullName, user.Email, subject, text);
        }

        public async Task SendSiteUpdateAsync(List<ApplicationUser> recepients, Update update, Site site)
        {
            string text = Mail.SiteUpdate(update.ToString(), site.Title, site.FriendlyId);
            string subject = Mail.SiteUpdateSubject(_mailSettings.SenderName, site.FriendlyId);
            await SendMailAsync(GetNameAddressPairs(recepients), subject, text);
        }
    }
}
