using GoApi.Data.Dtos;
using GoApi.Data.Models;
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
        /// If there is no update, then returns null.
        /// A resource is a Site or Job.
        /// </summary>
        /// <param name="userDetailLocation">Url for the detail of the user performing the update - to be used in hyperlinks on FE.</param>
        /// <returns></returns>
        Update GetResourceUpdate<T, U>(ApplicationUser user, T resource, U preUpdate, U postUpdate, string userDetailLocation)
            where T : class
            where U : class;



    }
}
