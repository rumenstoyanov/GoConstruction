using GoApi.Data.Constants;
using GoApi.Data.Models;
using GoApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Text.RegularExpressions;

namespace GoApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthService(JwtSettings jwtSettings, UserManager<ApplicationUser> userManager)
        {
            _jwtSettings = jwtSettings;
            _userManager = userManager;
        }
        public JwtSecurityToken GenerateJwtToken(Claim[] claims)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                notBefore: DateTime.Now,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.TokenLifetimeHours),
                claims: claims,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }

        public string GeneratePassword()
        {
            string password = null;
            bool isPasswordValid = false;
            var random = new Random();
            var regex = new Regex(Seniority.RandomPasswordRegex);

            while (!isPasswordValid)
            {
                password = new string(Enumerable.Repeat(Seniority.RandomPasswordChars, Seniority.RandomPasswordLength).Select(s => s[random.Next(s.Length)]).ToArray());
                isPasswordValid = regex.IsMatch(password);
            }

            return password;
        }

        public Guid GetRequestOid(HttpRequest request)
        {
            var jwt = request.Headers["Authorization"].ToString().Substring(7);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            string oid = token.Claims.Where(c => c.Type == Seniority.OrganisationIdClaimKey).ToList().First().Value;
            return Guid.Parse(oid);
        }

        public async Task<IEnumerable<ApplicationUser>> GetValidUsersAsync(Guid oid)
        {
            return (await _userManager.GetUsersForClaimAsync(new Claim(Seniority.OrganisationIdClaimKey, oid.ToString()))).Where(u => u.IsActive && u.EmailConfirmed && u.IsInitialSet);
        }
    }
}
