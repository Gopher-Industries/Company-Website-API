using Authentication.Models.Rest.Request;
using Authentication.Models.Rest.Response;
using Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Users.Interface;

namespace Authentication.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/auth")]
    [SwaggerTag(description: "<h3>Authentication Management Endpoint</h3>")]
    public class AuthenticationController : ControllerBase
    {

        private readonly IAuthenticationService AuthService;

        public AuthenticationController(IAuthenticationService AuthService)
        {
            this.AuthService = AuthService;
        }

        /// <summary>
        /// This is where users can login to their account
        /// </summary>
        /// <returns></returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The user was registered successfully", typeof(LoginResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, description: "Login credentials failed")]
        public async Task<ObjectResult> Register([FromBody] LoginRequest Request)
        {

            var Attempt = await this.AuthService.CreateUserAuthentication(Request.Username, Request.Password);

            if (Attempt is null)
                return Unauthorized(value: "Username or password was wrong.");

            return Ok(value: Attempt);

        }

        /// <summary>
        /// This is where users can login to their account
        /// </summary>
        /// <returns></returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The user was registered successfully", typeof(LoginResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, description: "Login credentials failed")]
        public async Task<ObjectResult> Login([FromBody] LoginRequest Request)
        {

            var Attempt = await this.AuthService.Login(Request.Username, Request.Password);

            if (Attempt is null)
                return Unauthorized(value: "Username or password was wrong.");

            return Ok(value: Attempt);

        }

        /// <summary>
        /// This is where users can use their refresh tokens to generate a new access token
        /// </summary>
        /// <returns>A JWT Token used for authentication for all API services. Bearer token schema.</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The authentication refresh was successful!", typeof(LoginResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, description: "The refresh token was invalid or has been revoked.")]
        public async Task<ObjectResult> RefreshLogin([FromBody] RefreshAccessTokenRequest Request)
        {

            //
            // Because we use a different JWT secret to sign refresh tokens, we have to manually validate it.
            // Ew.
            // Not too much work doe.

            var Attempt = AuthService.RefreshLogin(Request.RefreshToken);

            if (Attempt is null)
                return Unauthorized(value: "Refresh token invalid.");

            return Ok(value: Attempt);

        }

        /// <summary>
        /// An endpoint to validate the JWT token.
        /// </summary>
        /// <returns></returns>
        [HttpGet("validate")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The JWT token in use is valid and accepted by the server")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, description: "Failed to validate your token.")]
        public async Task<ObjectResult> Validate()
        {
            // Just list their claims, debug code really.
            return Ok(value: $"Here are your valid claims: \n{string.Join('\n', User.Claims.Select(x => $"\"{x.Type}\": \"{x.Value}\"").ToList())}");
        }

    }
}
