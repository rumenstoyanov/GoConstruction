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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace GoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IResourceService _resourceService;
        private readonly IUpdateService _updateService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _queue;
        public JobsController(
            AppDbContext appDbContext,
            IMapper mapper,
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IResourceService resourceService,
            IUpdateService updateService,
            IServiceScopeFactory serviceScopeFactory,
            IBackgroundTaskQueue queue
            )
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;
            _userManager = userManager;
            _resourceService = resourceService;
            _updateService = updateService;
            _serviceScopeFactory = serviceScopeFactory;
            _queue = queue;
        }



        [HttpGet]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public IActionResult GetJobs()
        {
            var oid = _authService.GetRequestOid(Request);
            var jobs = _appDbContext.Jobs.Where(j => j.Oid == oid && j.IsActive);
            var mappedJobs = _mapper.Map<IEnumerable<JobReadResponseDto>>(jobs);
            return Ok(mappedJobs);

        }

        [HttpGet("{jobId}", Name = nameof(GetJobsDetail))]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public async Task<IActionResult> GetJobsDetail(Guid jobId)
        {
            var oid = _authService.GetRequestOid(Request);
            var job = await _appDbContext.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive && j.Oid == oid);
            if (job != null)
            {
                var mappedJob = _mapper.Map<JobReadResponseDto>(job);
                return Ok(mappedJob);
            }
            return NotFound();
        }

        [HttpGet("{jobId}/children")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public IActionResult GetJobChildren(Guid jobId)
        {
            var oid = _authService.GetRequestOid(Request);
            var jobs = _appDbContext.Jobs.Where(j => j.Oid == oid && j.IsActive && j.ParentJobId.HasValue && j.ParentJobId.Value == jobId);
            var mappedJobs = _mapper.Map<IEnumerable<JobReadResponseDto>>(jobs);
            return Ok(mappedJobs);
        }

        [HttpPost]
        [Authorize(Policy = Seniority.SupervisorOrAbovePolicy)]
        public async Task<IActionResult> PostJobs([FromBody] NonRootJobCreateRequestDto model)
        {
            var oid = _authService.GetRequestOid(Request);
            var parentJob = await _appDbContext.Jobs.FirstOrDefaultAsync(j => j.Oid == oid && j.IsActive && j.Id == model.ParentJobId);
            if (parentJob != null)
            {
                var mappedJob = _mapper.Map<Job>(model);

                var user = await _userManager.GetUserAsync(User);
                var site = await _appDbContext.Sites.FirstOrDefaultAsync(s => s.Id == parentJob.SiteId);

                await _resourceService.CreateJobAsync(site, mappedJob, oid, user, false); // Not a root job, the parentJobId is already mapped in by the IMapper, so IsRoot = false.

                return CreatedAtRoute(nameof(GetJobsDetail), new { jobId = mappedJob.Id }, _mapper.Map<JobReadResponseDto>(mappedJob));
            }
            return NotFound();
        }

        [HttpDelete("{jobId}")]
        [Authorize(Policy = Seniority.ManagerOrAbovePolicy)]
        public async Task<IActionResult> DeleteJobs(Guid jobId)
        {
            var oid = _authService.GetRequestOid(Request);
            var job = await _appDbContext.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive && j.Oid == oid);
            if (job != null)
            {
                job.IsActive = false;
                await _appDbContext.SaveChangesAsync();
                return NoContent();
            }
            return NotFound();
        }

        [HttpPatch("{jobId}")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public async Task<IActionResult> PatchJobs(Guid jobId, [FromBody] JsonPatchDocument<JobUpdateRequestDto> patchDoc)
        {
            var oid = _authService.GetRequestOid(Request);
            var job = await _appDbContext.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive && j.Oid == oid);
            if (job != null)
            {
                var jobToPatch = _mapper.Map<JobUpdateRequestDto>(job);
                patchDoc.ApplyTo(jobToPatch, ModelState);
                if (!TryValidateModel(jobToPatch))
                {
                    return ValidationProblem(ModelState);
                }
                // Case of a non-foreign key JobStatusId.
                if (!await _appDbContext.JobStatuses.AnyAsync(js => js.Id == jobToPatch.JobStatusId))
                {
                    string errorKey = nameof(jobToPatch.JobStatusId);
                    ModelState.AddModelError(errorKey, $"Invalid {errorKey}.");
                    return ValidationProblem(ModelState);
                }

                var user = await _userManager.GetUserAsync(User);
                var update = _updateService.GetResourceUpdate(
                    user, 
                    job, 
                    _resourceService.GetJobUpdateFriendly(_mapper.Map<JobUpdateRequestDto>(job)),
                    _resourceService.GetJobUpdateFriendly(jobToPatch), 
                    _resourceService.GetUserDetailLocation(Url, Request, user)
                    );
                _mapper.Map(jobToPatch, job);

                if (update != null)
                {
                    _appDbContext.Add(update);

                    _queue.QueueBackgroundWorkItem(async token =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                            var updateService = scope.ServiceProvider.GetRequiredService<IUpdateService>();
                            var recepients = updateService.GetJobUpdateRecipients(job);
                            await mailService.SendJobUpdateAsync(recepients, update, job);
                        }
                    });
                }
                await _appDbContext.SaveChangesAsync();
                return NoContent();

            }
            return NotFound();
        }


    }
}
