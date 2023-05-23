using Authentication.Models.Settings;
using Authentication.Models.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Services
{

    public interface ITokenService
    {

        /// <summary>
        /// Generates a new token pair using the users information
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Username"></param>
        /// <param name="Role"></param>
        /// <returns></returns>
        public (AccessToken AccessToken, RefreshTokenStateful RefreshToken) GenerateNewTokenPair(string UserId, string Username, string Role);

        /// <summary>
        /// Generates a new token pair based on the previous refresh token
        /// </summary>
        /// <returns>The new access token and refresh token.</returns>
        public (AccessToken AccessToken, RefreshTokenStateful RefreshToken) GenerateNewTokenPair(RefreshTokenStateful Token);

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
        public RefreshTokenStateful? ReadRefreshToken(string RefreshToken);

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
        private readonly IOptions<ApplicationSecurityKeys> appSecurityKeys;

        public TokenService(IOptions<ApplicationHostSettings> hostSettings,
                            IOptions<ApplicationSecurityKeys> appIdentitySettings)
        {
            this.hostSettings = hostSettings;
            this.appSecurityKeys = appIdentitySettings;
        }

        public (AccessToken AccessToken, RefreshTokenStateful RefreshToken) GenerateNewTokenPair(RefreshTokenStateful Token)
        {
            return (this.BuildAccessToken(Token.UserId, Token.Username, Token.Role),
                    this.BuildRefreshToken(Token.UserId, Token.Username, Token.Role, Token.TokenId));
        }

        public (AccessToken AccessToken, RefreshTokenStateful RefreshToken) GenerateNewTokenPair(string UserId, string Username, string Role)
        {
            return (this.BuildAccessToken(UserId, Username, Role),
            this.BuildRefreshToken(UserId, Username, Role, Guid.NewGuid().ToString()));
        }

        private AccessToken BuildAccessToken(string UserId, string Username, string Role)
        {
            var Claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, UserId),
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Role, Role),
            };

            var RSAInstance = RSA.Create();
            RSAInstance.ImportFromPem(appSecurityKeys.Value.AccessTokenPrivateKey);
            var securityKey = new RsaSecurityKey(RSAInstance);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSsaPssSha512);
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

        private RefreshTokenStateful BuildRefreshToken(string UserId, string Username, string Role, string RefreshTokenId)
        {

            // Generate a brand new token secret for the new refresh token
            var RefreshTokenSecret = Encoding.ASCII.GetString(RandomNumberGenerator.GetBytes(RandomNumberGenerator.GetInt32(40, 60)));

            var Claims = new[] {
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.NameIdentifier, UserId),
                new Claim(ClaimTypes.Role, Role),
                new Claim("RefreshTokenId", RefreshTokenId),
                new Claim("RefreshTokenSecret", RefreshTokenSecret),
            };

            var RSAInstance = RSA.Create();
            RSAInstance.ImportFromPem(appSecurityKeys.Value.RefreshTokenPrivateKey);
            var securityKey = new RsaSecurityKey(RSAInstance);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSsaPssSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: hostSettings.Value.ExternalUrl,
                audience: hostSettings.Value.ExternalUrl,
                claims: Claims,
                expires: DateTime.Now.Add(RefreshTokenExpiery),
                signingCredentials: credentials);

            return new RefreshTokenStateful
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

        public RefreshTokenStateful? ReadRefreshToken(string RefreshToken)
        {

            // If the refresh token is invalid, return empty
            if (this.ValidateRefreshToken(RefreshToken) == false)
                return null;

            var RefreshTokenId = new JwtSecurityTokenHandler().ReadJwtToken(RefreshToken);

            if (RefreshTokenId is null)
                return null;

            return new RefreshTokenStateful()
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

            var RSAInstance = RSA.Create();
            RSAInstance.ImportFromPem(appSecurityKeys.Value.RefreshTokenPrivateKey);
            var securityKey = new RsaSecurityKey(RSAInstance);

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
                    IssuerSigningKey = securityKey
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
