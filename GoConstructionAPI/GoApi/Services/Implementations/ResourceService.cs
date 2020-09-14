using AutoMapper;
using GoApi.Controllers;
using GoApi.Data;
using GoApi.Data.Constants;
using GoApi.Data.Dtos;
using GoApi.Data.Models;
using GoApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;

namespace GoApi.Services.Implementations
{
    public class ResourceService : IResourceService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICacheService _cacheService;

        public ResourceService(AppDbContext appDbContext, IMapper mapper, UserManager<ApplicationUser> userManager, ICacheService cacheService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _userManager = userManager;
            _cacheService = cacheService;
            
        }

        public async Task CreateJobAsync(Site site, Job mappedJob, Guid oid, ApplicationUser user, bool IsRoot)
        {
           
            mappedJob.Oid = oid;
            mappedJob.OwnerId = user.Id;
            mappedJob.CreatedAt = DateTime.UtcNow;
            mappedJob.IsActive = true;
            mappedJob.SiteId = site.Id;
            mappedJob.FriendlyId = GenerateJobFriendlyId(site);
            mappedJob.JobStatusId = GetDefaultJobStatusId();
            if (IsRoot)
            {
                mappedJob.ParentJobId = null; // Root job so no ParentJobId, otherwise the ParentJobId would already by mapped in by IMapper in the Controller.
            }
            await _appDbContext.AddAsync(mappedJob);
            await _appDbContext.SaveChangesAsync();
        }

        private string GenerateJobFriendlyId(Site site)
        {
            int maxId = _appDbContext.Jobs.Where(j => j.SiteId == site.Id).Count();
            int newId = maxId + 1;
            return $"{site.FriendlyId}-{newId}";
        }

        private int GetDefaultJobStatusId()
        {
            var defaultStatus = _appDbContext.JobStatuses.SingleOrDefault(js => js.Title == JobStatuses.DefaultStatus);
            return defaultStatus.Id;

        }

        public string GetUserDetailLocation(IUrlHelper url, HttpRequest request, string userId)
        {
            var location = url.Action(nameof(OrganisationController.GetUsersDetail), "Organisation", new { userId = userId }, request.Scheme);
            return location;
        }

        public JobUpdateInternalDto GetJobUpdateFriendly(JobUpdateRequestDto dto)
        {
            var mappedDto = _mapper.Map<JobUpdateInternalDto>(dto);
            mappedDto.Status = _appDbContext.JobStatuses.FirstOrDefault(js => js.Id == dto.JobStatusId).Title;
            return mappedDto;
        }
        
        public IEnumerable<UserJob> GetAssigneeUserIdsForValidJob(Guid jobId)
        {
            return _appDbContext.Assignments.Where(uj => uj.JobId == jobId);
        }

        public async Task<AbridgedUserInfoResponseDto> GetAbridgedUserInfoFromUserIdAsync(string userId, IUrlHelper url, HttpRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var location = GetUserDetailLocation(url, request, user.Id);
            return new AbridgedUserInfoResponseDto { Id = user.Id, FullName = user.FullName, Location = location };
        }

        public async Task<List<AbridgedUserInfoResponseDto>> GetAbridgedUserInfoFromUserIdAsync(List<string> userIds, IUrlHelper url, HttpRequest request)
        {
            var outList = new List<AbridgedUserInfoResponseDto>();
            foreach (var userId in userIds)
            {
                outList.Add(await GetAbridgedUserInfoFromUserIdAsync(userId, url, request));
            }
            return outList;
        }

        public bool IsNewSiteFriendlyIdValid(SiteCreateRequestDto dto, Guid oid)
        {
            return !_appDbContext.Sites.Any(s => s.IsActive && s.Oid == oid && s.FriendlyId == dto.FriendlyId);
        }

        public async Task FlushCacheForNewSiteAsync(HttpRequest request, Guid oid)
        {
            // The endpoint for creating a new site and getting all sites is the same so the request path generates the same cache key.
            await _cacheService.TryDeleteCacheValueAsync(request, oid);
        }

        public async Task FlushCacheForSiteMutationAsync(HttpRequest request, IUrlHelper url, Guid oid)
        {
            // The endpoint to get this site is the same as any mutation endpoint so generates the same cache key.
            await _cacheService.TryDeleteCacheValueAsync(request, oid);

            // The get all sites endpoint.
            var location = url.Action(nameof(SitesController.GetSites), "Sites", null, request.Scheme);
            await _cacheService.TryDeleteCacheValueAsync(_cacheService.BuildCacheKeyFromUrl(location, oid));
        }
    }
}
