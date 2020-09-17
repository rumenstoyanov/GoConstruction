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
        /// <summary>
        /// Mutation is PUT, PATCH or DELETE.
        /// </summary>
        Task FlushCacheForSiteMutationAsync(HttpRequest request, IUrlHelper url, Guid oid);

        Task FlushCacheForNewRootJobAsync(HttpRequest request, IUrlHelper url, Guid oid);
        Task FlushCacheForNewNonRootJobAsync(HttpRequest request, IUrlHelper url, Guid oid, Guid parentJobId);
        /// <summary>
        /// Mutation is PUT, PATCH or DELETE. Note that all Jobs are mutated at the same endpoints.
        /// </summary>
        Task FlushCacheForJobMutationAsync(HttpRequest request, IUrlHelper url, Guid oid, Job job);

        Task FlushCacheForNewUserAsync(HttpRequest request, IUrlHelper url, Guid oid);

    }
}
