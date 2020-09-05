using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GoApi.Data;
using GoApi.Data.Constants;
using GoApi.Data.Dtos;
using GoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public JobsController(
            AppDbContext appDbContext,
            IMapper mapper,
            IAuthService authService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;
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
    }
}
