using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Tokens.Models;
using Tokens.Models.Configuration;

namespace Tokens
{

    public interface ITokenValidationService
    {

        /// <summary>
        /// Read an access token from the encoded access token JWT
        /// </summary>
        /// <param name="AccessTokenJWT"></param>
        /// <returns></returns>
        public IAccessToken? ValidateAccessToken(string AccessTokenJWT, string ValidAudience = "");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Claims"></param>
        /// <returns></returns>
        public IAccessToken? ReadAccessToken(ClaimsPrincipal Claims);

    }

    public class TokenValidationService : ITokenValidationService
    {

        private readonly IOptions<TokenValidationServiceConfiguration> configuration;

        public TokenValidationService(IOptions<TokenValidationServiceConfiguration> configuration)
        {
            this.configuration = configuration;
        }

        public IAccessToken? ReadAccessToken(ClaimsPrincipal Claims)
        {
            return new AccessToken
            {
                UserId = Claims.FindFirstValue(ClaimTypes.NameIdentifier),
                Username = Claims.FindFirstValue(ClaimTypes.Name),
                Role = Claims.FindFirstValue(ClaimTypes.Role)
            };
        }

        public IAccessToken? ValidateAccessToken(string AccessTokenJWT, string ValidAudience = "")
        {

            //
            // We validate the signature of the JWT is correct before continuing. 
            if (ValidateAccessTokenSignature(AccessTokenJWT, configuration.Value.AccessTokenPublicKey, configuration.Value.TokenIssuer, ValidAudience) is false)
                return null;

            var RefreshTokenModel = new JwtSecurityTokenHandler().ReadJwtToken(AccessTokenJWT);

            if (RefreshTokenModel is null)
                return null;

            return new AccessToken()
            {
                UserId = RefreshTokenModel.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                Username = RefreshTokenModel.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value,
                Role = RefreshTokenModel.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
                SignedJWT = AccessTokenJWT,
            };

        }

        public static bool ValidateAccessTokenSignature(string AccessToken, string PublicKey, string ValidIssuer, string ValidAudience)
        {

            var RSAInstance = RSA.Create();
            RSAInstance.ImportFromPem(PublicKey);
            var securityKey = new RsaSecurityKey(RSAInstance);

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(AccessToken, new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = ValidIssuer,
                    ValidAudience = ValidAudience,
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
