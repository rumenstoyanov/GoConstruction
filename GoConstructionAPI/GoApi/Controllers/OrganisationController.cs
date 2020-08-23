using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoApi.Data.Constants;
using Microsoft.AspNetCore.Identity;
using GoApi.Data.Models;
using GoApi.Data;
using AutoMapper;
using GoApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using GoApi.Data.Dtos;

namespace GoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
    public class OrganisationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserDbContext _userDbContext;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;


        public OrganisationController(
            UserManager<ApplicationUser> userManager,
            UserDbContext userDbContext,
            AppDbContext appDbContext,
            IMapper mapper,
            IAuthService authService
            )
        {
            _userManager = userManager;
            _userDbContext = userDbContext;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;

        }

        [HttpGet]
        [Route("info")]
        public async Task<IActionResult> GetOrganisationInfo()
        {
            var oid = _authService.GetRequestOid(Request);
            var org = await _appDbContext.Organisations.FirstOrDefaultAsync(o => o.Id == oid);
            if (org != null)
            {
                return Ok(_mapper.Map<OrganisationInfoResponsetDto>(org));
            }
            return BadRequest();
            
        }
    }
}
