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
    }
}
