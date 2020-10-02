using GoApi.Data.Constants;
using GoLibrary.Data.Models;
using GoLibrary.Data.Internals;
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
        private readonly IMessagePublisher _messagePublisher;
        public MailService(MailSettings mailSettings, IMessagePublisher messagePublisher)
        {
            _mailSettings = mailSettings;
            _messagePublisher = messagePublisher;
        }

        public Dictionary<string, string> GetNameAddressPairs(IEnumerable<ApplicationUser> recepients)
        {
            var outDict = new Dictionary<string, string>();
            foreach (var r in recepients)
            {
                outDict[r.Email] = r.FullName; // The email is unique, the full name is not. Use dictionary indexer as opposed to .Add method to ensure no duplicate emails.
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

        public async Task SendJobUpdateAsync(IEnumerable<ApplicationUser> recepients, Update update, Job job)
        {
            string text = Mail.JobUpdate(update.ToString(), job.Title, job.FriendlyId);
            string subject = Mail.JobUpdateSubject(_mailSettings.SenderName, job.FriendlyId);
            await SendMailAsync(GetNameAddressPairs(recepients), subject, text);
        }

        public async Task SendMailAsync(string toName, string toAddress, string subject, string text)
        {
            if (!_mailSettings.IsEnabled)
            {
                return;
            }

            var email = new EmailMessageDto { ToName = toName, ToAddress = toAddress, Subject = subject, Text = text };
            await _messagePublisher.Publish(email);

            //var message = new MimeMessage();
            //message.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.Email));
            //message.To.Add(new MailboxAddress(toName, toAddress));
            //message.Subject = subject;
            //message.Body = new TextPart("plain")
            //{
            //    Text = text
            //};

            //using (var client = new SmtpClient())
            //{
            //    await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.Port);

            //    client.AuthenticationMechanisms.Remove("XOAUTH2");

            //    await client.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);

            //    await client.SendAsync(message);
            //    await client.DisconnectAsync(true);
            //}
        }

        public async Task SendMailAsync(Dictionary<string, string> emailNamePairs, string subject, string text)
        {
            var emailTasks = new List<Task>();
            foreach (var email in emailNamePairs.Keys)
            {
                emailTasks.Add(SendMailAsync(emailNamePairs[email], email, subject, text));
            }
            await Task.WhenAll(emailTasks);
        }

        public async Task SendResetPasswordEmailAsync(ApplicationUser user, string newPassword)
        {
            string text = Mail.ResetPasswordBody(user.FullName, _mailSettings.SenderName, newPassword);
            string subject = Mail.ResetPasswordSubject(_mailSettings.SenderName);
            await SendMailAsync(user.FullName, user.Email, subject, text);
        }

        public async Task SendSiteUpdateAsync(IEnumerable<ApplicationUser> recepients, Update update, Site site)
        {
            string text = Mail.SiteUpdate(update.ToString(), site.Title, site.FriendlyId);
            string subject = Mail.SiteUpdateSubject(_mailSettings.SenderName, site.FriendlyId);
            await SendMailAsync(GetNameAddressPairs(recepients), subject, text);
        }
    }
}
