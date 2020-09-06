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
    public interface IResourceService
    {
        Task CreateJobAsync(Site site, Job mappedJob, Guid oid, ApplicationUser user, bool IsRoot);

        string GetUserDetailLocation(IUrlHelper Url, HttpRequest Request, string userId);

        JobUpdateInternalDto GetJobUpdateFriendly(JobUpdateRequestDto dto);
    }
}
