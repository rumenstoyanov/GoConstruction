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

        /// <summary>
        /// Valid users are those with IsActive = True and EmailConfirmed = True. Those with IsInitialSet = False are still included in the list because
        /// this means they have verified email (if initial signup) or are in the process of a password reset - so still want to see them.
        /// </summary>
        Task<IEnumerable<ApplicationUser>> GetValidUsersAsync(Guid oid);

        string GeneratePassword();

        Task<AuthInternalDto> RegisterNonContractorAsync(RegisterNonContractorRequestDto model, HttpRequest Request, ClaimsPrincipal User, IUrlHelper Url, string seniority);

        ClaimsPrincipal IsJwtTokenValid(string accessToken);

        Task<LoginResponseDto> GenerateLoginResponse(ApplicationUser user);

        Task InvalidateAllUnusedRefreshTokens(ApplicationUser user);

        
    }
}
