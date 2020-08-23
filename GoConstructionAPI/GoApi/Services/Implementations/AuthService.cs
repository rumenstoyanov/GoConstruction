﻿using GoApi.Data.Constants;
using GoApi.Data.Models;
using GoApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GoApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        public AuthService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
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

        public Guid GetRequestOid(HttpRequest request)
        {
            var jwt = request.Headers["Authorization"].ToString().Substring(7);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            string oid = token.Claims.Where(c => c.Type == Seniority.OrganisationIdClaimKey).ToList().First().Value;
            return Guid.Parse(oid);
        }
    }
}