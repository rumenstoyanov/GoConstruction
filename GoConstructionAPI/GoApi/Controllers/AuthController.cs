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

namespace GoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserDbContext _userDbContext;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _queue;
        

        public AuthController(
            UserManager<ApplicationUser> userManager,
            UserDbContext userDbContext,
            AppDbContext appDbContext, 
            IMapper mapper,
            IAuthService authService,
            IServiceScopeFactory serviceScopeFactory,
            IBackgroundTaskQueue queue
            )
        {
            _userManager = userManager;
            _userDbContext = userDbContext;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;
            _serviceScopeFactory = serviceScopeFactory;
            _queue = queue;
        }


        [HttpPost]
        [Route("register/contractor")]
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
        

        [HttpPost]
        [Route("login")]
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
                    return BadRequest(new List<IdentityError> { new IdentityError { Code = "EmailNotConfirmed", Description = "Please confirm your email address." } });
                }

                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userClaims = await _userManager.GetClaimsAsync(user);
                    var userRoles = await _userManager.GetRolesAsync(user);

                    var claims = new[]
                    {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(userClaims.First().Type, userClaims.First().Value),
                    new Claim(Seniority.SeniorityClaimKey, userRoles.First()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(Seniority.IsInitalSetClaimKey, user.IsInitialSet.ToString())

                };

                    var token = _authService.GenerateJwtToken(claims);

                    return Ok(new LoginResponseDto
                    {
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        Expiration = token.ValidTo
                    });

                }
            }
            return Unauthorized();
        }

        [HttpGet]
        [Route("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest(new List<IdentityError> { new IdentityError { Code = "InvalidConfirmationLink", Description = "This is not a valid email confirmation link." } });
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new List<IdentityError> { new IdentityError { Code = "InvalidConfirmationLink", Description = "This is not a valid email confirmation link." } });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("changepassword")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors.ToList());
            }
        }

    }
}
