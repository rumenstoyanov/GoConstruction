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
using Microsoft.AspNetCore.Mvc;
using GoApi.Data.Dtos;
using System.Security.Policy;
using GoApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GoApi.Controllers;

namespace GoApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _queue;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthService(
            JwtSettings jwtSettings, 
            UserManager<ApplicationUser> userManager, 
            AppDbContext appDbContext,
            IServiceScopeFactory serviceScopeFactory,
            IBackgroundTaskQueue queue,
            TokenValidationParameters tokenValidationParameters
            )
        {
            _jwtSettings = jwtSettings;
            _appDbContext = appDbContext;
            _userManager = userManager;
            _serviceScopeFactory = serviceScopeFactory;
            _queue = queue;
            _tokenValidationParameters = tokenValidationParameters;
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
            return (await _userManager.GetUsersForClaimAsync(new Claim(Seniority.OrganisationIdClaimKey, oid.ToString()))).Where(u => u.IsActive && u.EmailConfirmed);
        }

        public ClaimsPrincipal IsJwtTokenValid(string accessToken)
        {
            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(accessToken, _tokenValidationParameters, out var validatedToken);
                if (IsSecurityAlgorithmValid(validatedToken))
                {
                    return claimsPrincipal;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool IsSecurityAlgorithmValid(SecurityToken validatedToken)
        {
            if ((validatedToken is JwtSecurityToken jwtSecurityToken) && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public async Task<AuthInternalDto> RegisterNonContractorAsync(RegisterNonContractorRequestDto model, HttpRequest Request, ClaimsPrincipal User, IUrlHelper Url, string seniority)
        {
            var oid = GetRequestOid(Request);
            var inviter = await _userManager.GetUserAsync(User);

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName,
                IsActive = true,
                IsInitialSet = false,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            string password = GeneratePassword();
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new Claim(Seniority.OrganisationIdClaimKey, oid.ToString()));
                await _userManager.AddToRoleAsync(user, seniority);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(AuthController.ConfirmEmail), "Auth", new { userId = user.Id, token = token }, Request.Scheme);

                var org = await _appDbContext.Organisations.FirstOrDefaultAsync(o => o.Id == oid);

                _queue.QueueBackgroundWorkItem(async token =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                        await mailService.SendConfirmationEmailAndPasswordNonContractorAsync(org, user, inviter, seniority, confirmationLink, password);
                    }
                });
                return new AuthInternalDto { Success = true };
            }
            else
            {
                return new AuthInternalDto { Success = false, Errors = result.Errors.ToList() };
            }
        }

        public async Task<LoginResponseDto> GenerateLoginResponse(ApplicationUser user)
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

            var accessToken = GenerateJwtToken(claims);

            var refreshToken = new RefreshToken
            {
                jti = accessToken.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsUsed = false,
                IsInvalidated = false,
                UserId = user.Id
            };

            await _appDbContext.AddAsync(refreshToken);
            await _appDbContext.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                Expiration = accessToken.ValidTo,
                RefreshToken = refreshToken.Token.ToString()
            };
        }
    }
}
