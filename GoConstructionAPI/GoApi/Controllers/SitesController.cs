using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GoApi.Data;
using GoApi.Data.Constants;
using GoApi.Data.Dtos;
using GoApi.Data.Models;
using GoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SitesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IUpdateService _updateService;
        private readonly IResourceService _resourceService;
        private readonly ICacheService _cacheService;


        public SitesController(
            UserManager<ApplicationUser> userManager,
            AppDbContext appDbContext,
            IMapper mapper,
            IAuthService authService,
            IServiceScopeFactory serviceScopeFactory,
            IBackgroundTaskQueue queue,
            IUpdateService updateService,
            IResourceService resourceService,
            ICacheService cacheService
            
            )
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;
            _serviceScopeFactory = serviceScopeFactory;
            _queue = queue;
            _updateService = updateService;
            _resourceService = resourceService;
            _cacheService = cacheService;
        }

        [HttpPost]
        [Authorize(Policy = Seniority.ContractorOrAbovePolicy)]
        public async Task<IActionResult> PostSites([FromBody] SiteCreateRequestDto model)
        {
            var oid = _authService.GetRequestOid(Request);
            if (!_resourceService.IsNewSiteFriendlyIdValid(model, oid))
            {
                string errorKey = nameof(model.FriendlyId);
                ModelState.AddModelError(errorKey, "There already exists a Site with this ID.");
                return ValidationProblem(ModelState);
            }

            var mappedSite = _mapper.Map<Site>(model);
            var user = await _userManager.GetUserAsync(User);
            mappedSite.Oid = oid;
            mappedSite.CreatedByUserId = user.Id;
            mappedSite.CreatedAt = DateTime.UtcNow;
            mappedSite.IsActive = true;

            await _appDbContext.AddAsync(mappedSite);
            await _appDbContext.SaveChangesAsync();
            await _resourceService.FlushCacheForNewSiteAsync(Request, oid);

            return CreatedAtRoute(nameof(GetSitesDetail), new { siteId = mappedSite.Id }, _mapper.Map<SiteReadResponseDto>(mappedSite));

        }

        [HttpGet]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public async Task<IActionResult> GetSites()
        {
            var oid = _authService.GetRequestOid(Request);
            var fromCache = await _cacheService.TryGetCacheValueAsync<IEnumerable<SiteReadResponseDto>>(Request, oid);
            if (fromCache != null)
            {
                return Ok(fromCache);
            }

            var sites = _appDbContext.Sites.Where(s => s.Oid == oid && s.IsActive);
            var mappedSites = _mapper.Map<IEnumerable<SiteReadResponseDto>>(sites);
            await _cacheService.SetCacheValueAsync(Request, oid, mappedSites);
            return Ok(mappedSites);

        }

        [HttpGet("{siteId}", Name = nameof(GetSitesDetail))]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public async Task<IActionResult> GetSitesDetail(Guid siteId)
        {
            var oid = _authService.GetRequestOid(Request);
            var fromCache = await _cacheService.TryGetCacheValueAsync<SiteReadResponseDto>(Request, oid);
            if (fromCache != null)
            {
                return Ok(fromCache);
            }

            var site = await _appDbContext.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.IsActive && s.Oid == oid);
            if (site != null)
            {
                var mappedSite = _mapper.Map<SiteReadResponseDto>(site);
                await _cacheService.SetCacheValueAsync(Request, oid, mappedSite);
                return Ok(mappedSite);
            }
            return NotFound();
        }

        [HttpDelete("{siteId}")]
        [Authorize(Policy = Seniority.ContractorOrAbovePolicy)]
        public async Task<IActionResult> DeleteSites(Guid siteId)
        {
            var oid = _authService.GetRequestOid(Request);
            var site = await _appDbContext.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.IsActive && s.Oid == oid);
            if (site != null)
            {
                site.IsActive = false;
                await _appDbContext.SaveChangesAsync();
                await _resourceService.FlushCacheForSiteMutationAsync(Request, Url, oid);
                return NoContent();
            }
            return NotFound();
        }

        [HttpPatch("{siteId}")]
        [Authorize(Policy = Seniority.ManagerOrAbovePolicy)]
        public async Task<IActionResult> PatchSites(Guid siteId, JsonPatchDocument<SiteUpdateRequestDto> patchDoc)
        {
            var oid = _authService.GetRequestOid(Request);
            var site = await _appDbContext.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.IsActive && s.Oid == oid);
            if (site != null)
            {
                var siteToPatch = _mapper.Map<SiteUpdateRequestDto>(site);
                patchDoc.ApplyTo(siteToPatch, ModelState);
                if (!TryValidateModel(siteToPatch))
                {
                    return ValidationProblem(ModelState);
                }
                var user = await _userManager.GetUserAsync(User);
                var update = _updateService.GetResourceUpdate(user, site, _mapper.Map<SiteUpdateRequestDto>(site), siteToPatch);
                _mapper.Map(siteToPatch, site);

                if (update != null)
                {
                    _appDbContext.Add(update);

                    _queue.QueueBackgroundWorkItem(async token =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                            var updateService = scope.ServiceProvider.GetRequiredService<IUpdateService>();
                            var recepients = await updateService.GetSiteUpdateRecipientsAsync(site);
                            await mailService.SendSiteUpdateAsync(recepients, update, site);
                        }
                    });
                }
                await _appDbContext.SaveChangesAsync();
                return NoContent();
            }
            return NotFound();
        }


        [HttpPost("{siteId}/jobs")]
        [Authorize(Policy = Seniority.ManagerOrAbovePolicy)]
        public async Task<IActionResult> PostRootJobs(Guid siteId, [FromBody] RootJobCreateRequestDto model)
        {
            var oid = _authService.GetRequestOid(Request);
            var site = await _appDbContext.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.IsActive && s.Oid == oid);
            if (site != null)
            {
                var mappedJob = _mapper.Map<Job>(model);

                var user = await _userManager.GetUserAsync(User);

                await _resourceService.CreateJobAsync(site, mappedJob, oid, user, true);

                return CreatedAtRoute(nameof(JobsController.GetJobsDetail), new { jobId = mappedJob.Id }, _mapper.Map<JobReadResponseDto>(mappedJob));
            }

            return NotFound();
        }


        [HttpGet("{siteId}/jobs")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public IActionResult GetRootJobs(Guid siteId)
        {
            var oid = _authService.GetRequestOid(Request);
            // Need valid oid, siteId, active and a null parentJobId as seek root jobs only.
            var jobs = _appDbContext.Jobs.Where(j => j.Oid == oid && j.IsActive && j.SiteId == siteId && !j.ParentJobId.HasValue);
            var mappedJobs = _mapper.Map<IEnumerable<JobReadResponseDto>>(jobs);
            return Ok(mappedJobs);
        }
    }
}
