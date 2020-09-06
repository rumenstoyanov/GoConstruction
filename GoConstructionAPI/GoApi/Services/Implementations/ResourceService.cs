using AutoMapper;
using GoApi.Controllers;
using GoApi.Data;
using GoApi.Data.Constants;
using GoApi.Data.Dtos;
using GoApi.Data.Models;
using GoApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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

        public ResourceService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
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

        public string GetUserDetailLocation(IUrlHelper Url, HttpRequest Request, string userId)
        {
            var location = Url.Action(nameof(OrganisationController.GetUsersDetail), "Organisation", new { userId = userId }, Request.Scheme);
            return location;
        }

        public JobUpdateInternalDto GetJobUpdateFriendly(JobUpdateRequestDto dto)
        {
            var mappedDto = _mapper.Map<JobUpdateInternalDto>(dto);
            mappedDto.Status = _appDbContext.JobStatuses.FirstOrDefault(js => js.Id == dto.JobStatusId).Title;
            return mappedDto;
        }
    }
}
