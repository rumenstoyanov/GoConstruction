using GoApi.Data.Dtos;
using GoApi.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GoApi.Services.Interfaces
{
    public interface IAuthService
    {
        JwtSecurityToken GenerateJwtToken(Claim[] claims);

        Guid GetRequestOid(HttpRequest request);

        Task<IEnumerable<ApplicationUser>> GetValidUsersAsync(Guid oid);

        string GeneratePassword();

        Task<AuthInternalDto> RegisterNonContractorAsync(RegisterNonContractorRequestDto model, HttpRequest Request, ClaimsPrincipal User, IUrlHelper Url, string seniority);

        string GetUserDetailLocation(IUrlHelper Url, HttpRequest Request, ApplicationUser user);
    }
}
