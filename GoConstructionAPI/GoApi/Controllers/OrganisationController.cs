using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoApi.Data.Constants;
using Microsoft.AspNetCore.Identity;
using GoLibrary.Data.Models;
using GoLibrary.Data;
using GoApi.Data;
using AutoMapper;
using GoApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using GoLibrary.Data.Dtos;
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
        private readonly IResourceService _resourceService;
        private readonly ICacheService _cacheService;


        public OrganisationController(
            UserManager<ApplicationUser> userManager,
            AppDbContext appDbContext,
            IMapper mapper,
            IAuthService authService,
            IResourceService resourceService,
            ICacheService cacheService
            )
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;
            _resourceService = resourceService;
            _cacheService = cacheService;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetOrganisationInfo()
        {
            var oid = _authService.GetRequestOid(Request);
            var fromCache = await _cacheService.TryGetCacheValueAsync<OrganisationInfoResponsetDto>(Request, oid);
            if (fromCache != null)
            {
                return Ok(fromCache);
            }

            var org = await _appDbContext.Organisations.FirstOrDefaultAsync(o => o.Id == oid);
            if (org != null)
            {
                await _cacheService.SetCacheValueAsync(Request, oid, org);
                return Ok(_mapper.Map<OrganisationInfoResponsetDto>(org));
            }
            return BadRequest();
            
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var oid = _authService.GetRequestOid(Request);
            var fromCache = await _cacheService.TryGetCacheValueAsync<IEnumerable<UserInfoResponseDto>>(Request, oid);
            if (fromCache != null)
            {
                return Ok(fromCache);
            }

            var users = await _authService.GetValidUsersAsync(oid);
            var mappedUsers = new List<UserInfoResponseDto>();
            foreach (var user in users)
            {
                var mappedUser = _mapper.Map<UserInfoResponseDto>(user);
                mappedUser.Position = (await _userManager.GetRolesAsync(user)).First();
                mappedUsers.Add(mappedUser);
            }

            await _cacheService.SetCacheValueAsync(Request, oid, mappedUsers);
            return Ok(mappedUsers);
        }

        [HttpGet("users/abridged")]
        public async Task<IActionResult> GetUsersAbridged()
        {
            var oid = _authService.GetRequestOid(Request);
            var fromCache = await _cacheService.TryGetCacheValueAsync<IEnumerable<AbridgedUserInfoResponseDto>>(Request, oid);
            if (fromCache != null)
            {
                return Ok(fromCache);
            }

            var users = await _authService.GetValidUsersAsync(oid);
            var mappedUsers = new List<AbridgedUserInfoResponseDto>();
            foreach (var user in users)
            {
                var mappedUser = _mapper.Map<AbridgedUserInfoResponseDto>(user);
                mappedUser.Location = _resourceService.GetUserDetailLocation(Url, Request, user.Id);
                mappedUsers.Add(mappedUser);
            }

            await _cacheService.SetCacheValueAsync(Request, oid, mappedUsers);
            return Ok(mappedUsers);
        }

        [HttpGet("users/me")]
        public async Task<IActionResult> GetUsersDetailMe()
        {
            var user = await _userManager.GetUserAsync(User);
            var mappedUser = _mapper.Map<UserInfoResponseDto>(user);
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
                var mappedUser = _mapper.Map<UserInfoResponseDto>(user);
                mappedUser.Position = (await _userManager.GetRolesAsync(user)).First();
                return Ok(mappedUser);
            }

            return NotFound();
        }
    }
}
