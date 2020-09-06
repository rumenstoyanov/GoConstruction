using GoApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Interfaces
{
    public interface IMailService
    {
        Task SendMailAsync(string toName, string toAddress, string subject, string text);
        Task SendMailAsync(Dictionary<string, string> nameAddressPairs, string subject, string text);
        Task SendConfirmationEmailContractorAsync(Organisation org, ApplicationUser user, string confirmationLink);
        Task SendConfirmationEmailAndPasswordNonContractorAsync(Organisation org, ApplicationUser user, ApplicationUser inviter, string seniority, string confirmationLink, string password);
        Task SendResetPasswordEmailAsync(ApplicationUser user, string newPassword);
        Task SendJobUpdateAsync(List<ApplicationUser> recepients, Update update, Job job);
        Task SendSiteUpdateAsync(List<ApplicationUser> recepients, Update update, Site site);
        Dictionary<string, string> GetNameAddressPairs(List<ApplicationUser> recepients);
    }
}
