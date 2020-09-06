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
using System.Security.Claims;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;

namespace GoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
    public class OrganisationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;


        public OrganisationController(
            UserManager<ApplicationUser> userManager,
            AppDbContext appDbContext,
            IMapper mapper,
            IAuthService authService
            )
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;

        }

        [HttpGet("info")]
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

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var oid = _authService.GetRequestOid(Request);
            var users = await _authService.GetValidUsersAsync(oid);
            var mappedUsers = new List<ApplicationUserInfoResponseDto>();
            foreach (var user in users)
            {
                var mappedUser = _mapper.Map<ApplicationUserInfoResponseDto>(user);
                mappedUser.Position = (await _userManager.GetRolesAsync(user)).First();
                mappedUsers.Add(mappedUser);
            }
            return Ok(mappedUsers);
        }

        [HttpGet("users/me")]
        public async Task<IActionResult> GetUsersDetailMe()
        {
            var user = await _userManager.GetUserAsync(User);
            var mappedUser = _mapper.Map<ApplicationUserInfoResponseDto>(user);
            mappedUser.Position = (await _userManager.GetRolesAsync(user)).First();
            return Ok(mappedUser);

        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUsersDetail(string userId)
        {
            var oid = _authService.GetRequestOid(Request);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            var claims = await _userManager.GetClaimsAsync(user);

            if (claims.Any(c => c.Type == Seniority.OrganisationIdClaimKey && c.Value == oid.ToString()) && user.IsActive && user.EmailConfirmed)
            {
                var mappedUser = _mapper.Map<ApplicationUserInfoResponseDto>(user);
                mappedUser.Position = (await _userManager.GetRolesAsync(user)).First();
                return Ok(mappedUser);
            }

            return NotFound();
        }
    }
}
