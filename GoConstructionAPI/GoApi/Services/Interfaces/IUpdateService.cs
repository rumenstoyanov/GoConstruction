using GoApi.Data.Dtos;
using GoApi.Data.Models;
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

        Update GetSiteUpdate(ApplicationUser user, Site site, SiteUpdateRequestDto preUpdate, SiteUpdateRequestDto postUpdate);

        List<ApplicationUser> GetSiteUpdateRecipients(Site site);



    }
}
