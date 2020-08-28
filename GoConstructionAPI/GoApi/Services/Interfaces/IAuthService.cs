using GoApi.Data.Models;
using Microsoft.AspNetCore.Http;
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
    }
}
