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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public JobsController(
            AppDbContext appDbContext,
            IMapper mapper,
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IResourceService resourceService
            )
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;
            _userManager = userManager;
            _resourceService = resourceService;
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

    }
}
