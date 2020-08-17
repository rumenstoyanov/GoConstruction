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

        public AuthController(UserManager<ApplicationUser> userManager, UserDbContext userDbContext, AppDbContext appDbContext, IMapper mapper)
        {
            _userManager = userManager;
            _userDbContext = userDbContext;
            _appDbContext = appDbContext;
            _mapper = mapper;
        }


        [HttpPost]
        [Route("register/contractor")]
        public async Task<IActionResult> Register([FromBody] RegisterContractorDto model)
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
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.UserName,
                    IsActive = true,
                    PhoneNumber = model.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var org = _mapper.Map<Organisation>(model);
                    await _appDbContext.Organisations.AddAsync(org);
                    await _appDbContext.SaveChangesAsync();

                    await _userManager.AddClaimAsync(user, new Claim(Seniority.OrganisationIdClaimKey, org.Id.ToString()));
                    await _userManager.AddToRoleAsync(user, Seniority.Contractor);
                    return Ok();
                }
                else
                {
                    return BadRequest(result.Errors.ToList());
                }
            }
            return BadRequest();
        }
    }
}
