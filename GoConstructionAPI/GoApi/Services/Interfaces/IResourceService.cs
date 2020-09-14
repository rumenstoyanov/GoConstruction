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

        string GetUserDetailLocation(IUrlHelper url, HttpRequest request, string userId);

        JobUpdateInternalDto GetJobUpdateFriendly(JobUpdateRequestDto dto);

        IEnumerable<UserJob> GetAssigneeUserIdsForValidJob(Guid jobId);

        /// <summary>
        /// userId known to be valid a priori.
        /// </summary>
        Task<AbridgedUserInfoResponseDto> GetAbridgedUserInfoFromUserIdAsync(string userId, IUrlHelper url, HttpRequest request);

        Task<List<AbridgedUserInfoResponseDto>> GetAbridgedUserInfoFromUserIdAsync(List<string> userIds, IUrlHelper url, HttpRequest request);

        bool IsNewSiteFriendlyIdValid(SiteCreateRequestDto dto, Guid oid);

        Task FlushCacheForNewSiteAsync(HttpRequest request, Guid oid);
        Task FlushCacheForSiteMutationAsync(HttpRequest request, IUrlHelper url, Guid oid);

    }
}
