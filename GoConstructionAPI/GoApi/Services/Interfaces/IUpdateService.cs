using GoApi.Data.Dtos;
using GoApi.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Interfaces
{
    public interface IUpdateService
    {
        Dictionary<string, string> Diff<T>(T preUpdate, T postUpdate) where T : class;

        string AssembleSyntaxFromDiff(Dictionary<string, string> diff);

        List<ApplicationUser> GetSiteUpdateRecipients(Site site);

        /// <summary>
        /// A resource here is a Site or Job. If there is no update, then returns null.
        /// </summary>
        /// <param name="userDetailLocation">Url for the detail of the user performing the update - to be used in hyperlinks on FE.</param>
        /// <returns></returns>
        Update GetResourceUpdate<T, U>(ApplicationUser user, T resource, U preUpdate, U postUpdate, string userDetailLocation)
            where T : class
            where U : class;

        Task<IEnumerable<ApplicationUser>> GetJobUpdateRecipientsAsync(Job job);

        Update GetAssigneeUpdate(ApplicationUser user, Job job, ApplicationUser updatedUser, string userDetailLocation, string updatedUserDetailLocation, bool isAddition);


        Task<Update> GetCommentUpdateAsync(ApplicationUser user, Job job, Comment comment);

        void RemapLocationLink(UpdateDetail updateDetail, IUrlHelper Url, HttpRequest Request);
        void RemapLocationLink(Update update, IUrlHelper Url, HttpRequest Request);


    }
}
