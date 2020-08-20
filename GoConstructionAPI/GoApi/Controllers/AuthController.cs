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
        private readonly IMailService _mailService;

        public AuthController(UserManager<ApplicationUser> userManager, UserDbContext userDbContext, AppDbContext appDbContext, IMapper mapper, IAuthService authService, IMailService mailService)
        {
            _userManager = userManager;
            _userDbContext = userDbContext;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _authService = authService;
            _mailService = mailService;
        }


        [HttpPost]
        [Route("register/contractor")]
        public async Task<IActionResult> Register([FromBody] RegisterContractorRequestDto model)
        {
            if (ModelState.IsValid)
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
                    await _mailService.SendConfirmationEmailContractorAsync(org, user, confirmationLink);
                    return Ok();
                }
                else
                {
                    return BadRequest(result.Errors.ToList());
                }
            }
            return BadRequest();
        }

        [HttpPost]
        [Route("login/")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
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
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(userClaims[0].Type, userClaims[0].Value),
                    new Claim(Seniority.SeniorityClaimKey, userRoles[0])
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
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, string token)
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
    }
}
