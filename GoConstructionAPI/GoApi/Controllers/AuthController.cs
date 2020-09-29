using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GoApi.Data;
using GoApi.Data.Dtos;
using GoApi.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GoApi.Data.Constants;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GoApi.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IResourceService _resourceService;
        

        public AuthController(
            UserManager<ApplicationUser> userManager,
            AppDbContext appDbContext, 
            IMapper mapper,
            IAuthService authService,
            IServiceScopeFactory serviceScopeFactory,
            IBackgroundTaskQueue queue,
            IResourceService resourceService
            )
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;
            _serviceScopeFactory = serviceScopeFactory;
            _queue = queue;
            _resourceService = resourceService;
        }


        [HttpPost("register/contractor")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterContractor([FromBody] RegisterContractorRequestDto model)
        {
            if (_appDbContext.Organisations.Any(org => org.OrganisationName == model.OrganisationName))
            {
                return BadRequest(new List<IdentityError> { new IdentityError { Code = "OrganisationTaken", Description = "The given organisation name already exists." } });
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName,
                IsActive = true,
                IsInitialSet = true,
                PhoneNumber = model.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var org = _mapper.Map<Organisation>(model); // The model is certainly mappable since the Required properties are identical in the input DTO.
                await _appDbContext.Organisations.AddAsync(org);
                await _appDbContext.SaveChangesAsync();

                await _userManager.AddClaimAsync(user, new Claim(Seniority.OrganisationIdClaimKey, org.Id.ToString()));
                await _userManager.AddToRoleAsync(user, Seniority.Contractor);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token = token }, Request.Scheme);

                _queue.QueueBackgroundWorkItem(async token =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                        await mailService.SendConfirmationEmailContractorAsync(org, user, confirmationLink);
                    }
                });

                return Ok();
            }
            else
            {
                return BadRequest(result.Errors.ToList());
            }
        }
        

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                if (!user.IsActive)
                {
                    return Unauthorized();
                }
                if (!user.EmailConfirmed)
                {
                    return BadRequest(new List<IdentityError> { new IdentityError { Code = Messages.EmailNotConfirmedCode, Description = Messages.EmailNotConfirmedDescription } });
                }

                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return Ok(await _authService.GenerateLoginResponse(user));
                }
            }
            return Unauthorized();
        }

        /// <summary>
        /// We allow refreshing of access tokens that have not yet expired.
        /// Client middleware should redirect user to re-login for all response status codes other than 200.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto model)
        {
            var claimsPrincipal = _authService.IsJwtTokenValid(model.AccessToken);
            
            if (claimsPrincipal == null)
            {
                // Case of invalid (malformed) access token.
                return BadRequest();
            }

            // Claim that uniquely identifies a JWT token.
            var jti = claimsPrincipal.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token.ToString() == model.RefreshToken);

            if (
                (storedRefreshToken == null) || // Case of invalid refresh token.
                (DateTime.UtcNow > storedRefreshToken.ExpiryDate) || // Case of expired refresh token.
                (storedRefreshToken.jti != jti) || // Case of refresh token and access token each belonging to a different user.
                (storedRefreshToken.IsInvalidated) || // Case of invalidated refresh token.
                (storedRefreshToken.IsUsed) // Case of already used refresh token.
                )
            {
                return BadRequest();
            }

            // Mark the refresh token as used at this stage.
            storedRefreshToken.IsUsed = true;
            await _appDbContext.SaveChangesAsync();

            // NameIdentifier is the user id claim.
            var user = await _userManager.FindByIdAsync(claimsPrincipal.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);

            return Ok(await _authService.GenerateLoginResponse(user));



        }

        [HttpGet("confirmemail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost("setinitial")]
        [Authorize]
        public async Task<IActionResult> SetInitial([FromBody] SetInitialRequestDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.IsInitialSet || !user.IsActive || !user.EmailConfirmed)
            {
                return BadRequest();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                user.IsInitialSet = true;
                user.PhoneNumber = model.PhoneNumber;
                await _appDbContext.SaveChangesAsync();
                await _resourceService.FlushCacheForNewUserAsync(Request, Url, _authService.GetRequestOid(Request));
                await _authService.InvalidateAllUnusedRefreshTokens(user);
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors.ToList());
            }



        }

        [HttpPost("changepassword")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!user.IsActive || !user.EmailConfirmed)
            {
                return BadRequest();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _authService.InvalidateAllUnusedRefreshTokens(user);
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors.ToList());
            }
        }

        [HttpPost("register/manager")]
        [Authorize(Policy = Seniority.ContractorOrAbovePolicy)]
        public async Task<IActionResult> RegisterManager([FromBody] RegisterNonContractorRequestDto model)
        {
            var result = await _authService.RegisterNonContractorAsync(model, Request, User, Url, Seniority.Manager);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("register/supervisor")]
        [Authorize(Policy = Seniority.ManagerOrAbovePolicy)]
        public async Task<IActionResult> RegisterSuperviosr([FromBody] RegisterNonContractorRequestDto model)
        {
            var result = await _authService.RegisterNonContractorAsync(model, Request, User, Url, Seniority.Supervisor);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("register/worker")]
        [Authorize(Policy = Seniority.SupervisorOrAbovePolicy)]
        public async Task<IActionResult> RegisterWorker([FromBody] RegisterNonContractorRequestDto model)
        {
            var result = await _authService.RegisterNonContractorAsync(model, Request, User, Url, Seniority.Worker);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("resetpassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                if (!user.IsInitialSet || !user.IsActive || !user.EmailConfirmed)
                {
                    return BadRequest();
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                string newPassword = _authService.GeneratePassword();
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (result.Succeeded)
                {
                    user.IsInitialSet = false;
                    await _appDbContext.SaveChangesAsync();
                    await _authService.InvalidateAllUnusedRefreshTokens(user);

                    _queue.QueueBackgroundWorkItem(async token =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                            await mailService.SendResetPasswordEmailAsync(user, newPassword);
                        }
                    });
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            return NotFound();
        }

    }
}
