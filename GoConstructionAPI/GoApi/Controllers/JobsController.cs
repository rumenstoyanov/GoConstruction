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
            // Anybody can patch a job but only Supervisors and above can change assignees. 
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
                    _resourceService.GetUserDetailLocation(Url, Request, user.Id)
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
                            var recepients = await updateService.GetJobUpdateRecipientsAsync(job);
                            await mailService.SendJobUpdateAsync(recepients, update, job);
                        }
                    });
                }
                await _appDbContext.SaveChangesAsync();
                return NoContent();

            }
            return NotFound();
        }

        [HttpGet("statuses")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public IActionResult GetJobStatuses()
        {
            return Ok(_mapper.Map<IEnumerable<JobStatusReadResponseDto>>(_appDbContext.JobStatuses));
        }

        [HttpGet("{jobId}/assignees")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public async Task<IActionResult> GetAssignees(Guid jobId)
        {
            var oid = _authService.GetRequestOid(Request);
            var job = await _appDbContext.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive && j.Oid == oid);
            if (job != null)
            {
                var assignees = new List<AbridgedUserInfoResponseDto>();
                foreach (var uj in _resourceService.GetAssigneeUserIdsForValidJob(jobId).ToList())
                {
                    // Want ALL users, not just active ones, as FE want to see legacy assignees for due diligence.
                    assignees.Add(await _resourceService.GetAbridgedUserInfoFromUserIdAsync(uj.UserId, Url, Request));
                    
                }
                return Ok(assignees);
            }
            return NotFound();
        }

        [HttpPost("{jobId}/assignees")]
        [Authorize(Policy = Seniority.SupervisorOrAbovePolicy)]
        public async Task<IActionResult> PostAssignees(Guid jobId, [FromBody] AddAssigneeRequestDto assignee)
        {
            var oid = _authService.GetRequestOid(Request);
            var job = await _appDbContext.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive && j.Oid == oid);
            if (job != null)
            {
                if ((await _authService.GetValidUsersAsync(oid)).Any(u => u.Id == assignee.UserId))
                {
                    if (!_resourceService.GetAssigneeUserIdsForValidJob(jobId).Any(uj => uj.UserId == assignee.UserId))
                    {
                        _appDbContext.Assignments.Add(new UserJob { UserId = assignee.UserId, JobId = jobId });

                        var user = await _userManager.GetUserAsync(User);
                        var updatedUser = await _userManager.FindByIdAsync(assignee.UserId);

                        var update = _updateService.GetAssigneeUpdate(
                            user,
                            job,
                            updatedUser,
                            true
                            );
                        _appDbContext.Add(update);

                        _queue.QueueBackgroundWorkItem(async token =>
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                                var updateService = scope.ServiceProvider.GetRequiredService<IUpdateService>();
                                var recepients = await updateService.GetJobUpdateRecipientsAsync(job);
                                await mailService.SendJobUpdateAsync(recepients, update, job);
                            }
                        });

                        await _appDbContext.SaveChangesAsync();
                    }
                    return Ok();
                }
            }
            return NotFound();
        }

        [HttpDelete("{jobId}/assignees")]
        [Authorize(Policy = Seniority.SupervisorOrAbovePolicy)]
        public async Task<IActionResult> DeleteAssignees(Guid jobId, [FromBody] AddAssigneeRequestDto assignee)
        {
            var oid = _authService.GetRequestOid(Request);
            var job = await _appDbContext.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive && j.Oid == oid);
            if (job != null)
            {
                var assignment = await _appDbContext.Assignments.FirstOrDefaultAsync(uj => uj.JobId == jobId && uj.UserId == assignee.UserId);
                if (assignment != null)
                {
                    _appDbContext.Assignments.Remove(assignment);


                    var user = await _userManager.GetUserAsync(User);
                    var updatedUser = await _userManager.FindByIdAsync(assignee.UserId);

                    var update = _updateService.GetAssigneeUpdate(
                        user,
                        job,
                        updatedUser,
                        false
                        );
                    _appDbContext.Add(update);

                    _queue.QueueBackgroundWorkItem(async token =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                            var updateService = scope.ServiceProvider.GetRequiredService<IUpdateService>();
                            var recepients = await updateService.GetJobUpdateRecipientsAsync(job);
                            await mailService.SendJobUpdateAsync(recepients, update, job);
                        }
                    });

                    await _appDbContext.SaveChangesAsync();
                    return NoContent();
                }

            }
            return NotFound();
        }

        [HttpPost("{jobId}/comments")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public async Task<IActionResult> PostComments(Guid jobId, [FromBody] CommentCreateRequestDto model)
        {
            var oid = _authService.GetRequestOid(Request);
            var job = await _appDbContext.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive && j.Oid == oid);
            if (job != null)
            {
                var mappedComment = _mapper.Map<Comment>(model);
                var user = await _userManager.GetUserAsync(User);
                mappedComment.PostedByUserId = user.Id;
                mappedComment.JobId = jobId;
                mappedComment.TimePosted = DateTime.UtcNow;

                var validUsersToTag = await _authService.GetValidUsersAsync(oid);
                mappedComment.UsersTagged = mappedComment.UsersTagged.Distinct().Where(id => validUsersToTag.Any(u => u.Id == id)).ToList();

                var update = await _updateService.GetCommentUpdateAsync(user, job, mappedComment);

                await _appDbContext.AddAsync(mappedComment);
                await _appDbContext.AddAsync(update);

                _queue.QueueBackgroundWorkItem(async token =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                        var updateService = scope.ServiceProvider.GetRequiredService<IUpdateService>();
                        var recepients = await updateService.GetJobUpdateRecipientsAsync(job);
                        await mailService.SendJobUpdateAsync(recepients, update, job);
                    }
                });
                await _appDbContext.SaveChangesAsync();
                return Ok();

            }
            return NotFound();
        }

        [HttpGet("{jobId}/comments")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public async Task<IActionResult> GetComments(Guid jobId)
        {
            var oid = _authService.GetRequestOid(Request);
            var job = await _appDbContext.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive && j.Oid == oid);
            if (job != null)
            {
                var commentsOut = new List<CommentReadResponseDto>();

                foreach (var comment in _appDbContext.Comments.Where(c => c.JobId == jobId).ToList())
                {
                    var mappedComment = _mapper.Map<CommentReadResponseDto>(comment);
                    mappedComment.PostedByUserInfo = await _resourceService.GetAbridgedUserInfoFromUserIdAsync(comment.PostedByUserId, Url, Request);
                    mappedComment.UsersTaggedInfo = await _resourceService.GetAbridgedUserInfoFromUserIdAsync(comment.UsersTagged, Url, Request);
                    commentsOut.Add(mappedComment);
                }
                return Ok(commentsOut.OrderBy(c => c.TimePosted));
            }
            return NotFound();
        }
    }
}
