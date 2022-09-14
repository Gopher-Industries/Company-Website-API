using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using ProjectX.WebAPI.Models;
using ProjectX.WebAPI.Models.Database.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProjectX.WebAPI.Services
{

    public interface ITokenService
    {

        /// <summary>
        /// Build an access token using the users identity
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public AccessToken BuildAccessToken(UserModel User);

        /// <summary>
        /// Build an access token using the users refresh token
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public AccessToken BuildAccessToken(RefreshToken Token);

        /// <summary>
        /// Build a brand new refresh token for the first time
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public RefreshToken BuildRefreshToken(UserModel User);

        /// <summary>
        /// Build a refresh token from another refresh token
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public RefreshToken BuildRefreshToken(RefreshToken Token);

        /// <summary>
        /// Read a refresh token from the current session
        /// </summary>
        /// <param name="Claims"></param>
        /// <returns></returns>
        public AccessToken? ReadAccessToken(ClaimsPrincipal Claims);

        /// <summary>
        /// Read refresh token from encoded string format.
        /// This also validates that the token is valid.
        /// </summary>
        /// <param name="RefreshToken"></param>
        /// <returns></returns>
        public RefreshToken? ReadRefreshToken(string RefreshToken);

        /// <summary>
        /// Validates a refresh token was signed by our service
        /// </summary>
        /// <param name="RefreshToken"></param>
        /// <returns></returns>
        public bool ValidateRefreshToken(string RefreshToken);

    }

    public class TokenService : ITokenService
    {

        private readonly TimeSpan AccessTokenExpiery = TimeSpan.FromMinutes(30);
        private readonly TimeSpan RefreshTokenExpiery = TimeSpan.FromDays(3);

        private readonly IOptions<ApplicationHostSettings> hostSettings;
        private readonly IOptions<ApplicationIdentitySettings> appIdentitySettings;

        public TokenService(IOptions<ApplicationHostSettings> hostSettings,
                            IOptions<ApplicationIdentitySettings> appIdentitySettings)
        {
            this.hostSettings = hostSettings;
            this.appIdentitySettings = appIdentitySettings;
        }

        public AccessToken BuildAccessToken(UserModel User)
        {

            return this.BuildAccessToken(
                UserId: User.UserId,
                Username: User.Username,
                Role: UserRole.Patient);

        }

        public AccessToken BuildAccessToken(RefreshToken Token)
        {

            return this.BuildAccessToken(
                UserId: Token.UserId,
                Username: Token.Username,
                Role: UserRole.Patient);

        }

        public RefreshToken BuildRefreshToken(UserModel User)
        {

            // We're building a refresh token for the first time.

            var RefreshTokenId = Guid.NewGuid().ToString();
            

            return this.BuildRefreshToken(
                UserId: User.UserId,
                Username: User.Username,
                RefreshTokenId: RefreshTokenId);
            
        }

        public RefreshToken BuildRefreshToken(RefreshToken Token)
        {

            // We're NOT building a refresh token for the first time.

            return this.BuildRefreshToken(
                UserId: Token.UserId,
                Username: Token.Username,
                RefreshTokenId: Token.TokenId);

        }

        private AccessToken BuildAccessToken(string UserId, string Username, string Role)
        {
            var Claims = new[] {
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Role, Role),
                new Claim(ClaimTypes.NameIdentifier, UserId)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appIdentitySettings.Value.AccessJWTSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: hostSettings.Value.ExternalUrl,
                audience: hostSettings.Value.ExternalUrl,
                claims: Claims,
                expires: DateTime.Now.Add(AccessTokenExpiery),
                signingCredentials: credentials);

            return new AccessToken
            {
                UserId = UserId,
                Username = Username,
                Role = Role,
                SignedJWT = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor)
            };

        }

        private RefreshToken BuildRefreshToken(string UserId, string Username, string RefreshTokenId)
        {

            // Generate a brand new token secret for the new refresh token
            var RefreshTokenSecret = Encoding.ASCII.GetString(RandomNumberGenerator.GetBytes(RandomNumberGenerator.GetInt32(40, 60)));

            var Claims = new[] {
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.NameIdentifier, UserId),
                new Claim("RefreshTokenId", RefreshTokenId),
                new Claim("RefreshTokenSecret", RefreshTokenSecret),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appIdentitySettings.Value.RefreshJWTSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: hostSettings.Value.ExternalUrl,
                audience: hostSettings.Value.ExternalUrl,
                claims: Claims,
                expires: DateTime.Now.Add(RefreshTokenExpiery),
                signingCredentials: credentials);

            return new RefreshToken
            {
                TokenId = RefreshTokenId,
                UserId = UserId,
                Username = Username,
                Secret = RefreshTokenSecret,
                ValidUntil = DateTime.UtcNow.Add(RefreshTokenExpiery),
                SignedJWT = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor)
            };
        }

        public AccessToken? ReadAccessToken(ClaimsPrincipal Claims)
        {
            return new AccessToken
            {
                UserId = Claims.FindFirstValue(ClaimTypes.NameIdentifier),
                Username = Claims.FindFirstValue(ClaimTypes.Name),
                Role = Claims.FindFirstValue(ClaimTypes.Role)
            };
        }

        public RefreshToken? ReadRefreshToken(string RefreshToken)
        {

            // If the refresh token is invalid, return empty
            if (this.ValidateRefreshToken(RefreshToken) == false)
                return null;

            var RefreshTokenId = new JwtSecurityTokenHandler().ReadJwtToken(RefreshToken);

            if (RefreshTokenId is null)
                return null;

            return new RefreshToken()
            { 
                UserId = RefreshTokenId.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                Username = RefreshTokenId.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value,
                TokenId = RefreshTokenId.Claims.FirstOrDefault(x => x.Type == "RefreshTokenId")?.Value,
                Secret = RefreshTokenId.Claims.FirstOrDefault(x => x.Type == "RefreshTokenSecret")?.Value,
                SignedJWT = RefreshToken 
            }; 

        }

        public bool ValidateRefreshToken(string RefreshToken)
        {
            
            try
            {
                new JwtSecurityTokenHandler().ValidateToken(RefreshToken, new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = hostSettings.Value.ExternalUrl,
                    ValidAudience = hostSettings.Value.ExternalUrl,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            appIdentitySettings.Value.RefreshJWTSecret
                    ))
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
            
            
        }

    }
}
