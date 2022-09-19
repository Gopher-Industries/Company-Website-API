using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectX.WebAPI.Models.Database.Authentication;
using ProjectX.WebAPI.Models.RestRequests.Request.Users;
using ProjectX.WebAPI.Services;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectX.WebAPI.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {

        private readonly IUserService UserService;
        private readonly IEmailService EmailService;
        private readonly IAuthenticationService AuthService;
        private readonly ITokenService TokenService;

        public UsersController(IUserService UserService,
                               ITokenService TokenService,
                               IEmailService EmailService,
                               IAuthenticationService AuthService)
        {
            this.UserService = UserService;
            this.TokenService = TokenService;
            this.EmailService = EmailService;
            this.AuthService = AuthService;
        }

        /// <summary>
        /// Retrieve information regarding a user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpGet("{UserId}", Name = "Get User")]
        public async Task<ObjectResult> GetUser([FromQuery] string UserId)
        {
            
            var AccessToken = this.TokenService.ReadAccessToken(this.HttpContext.User);

            // If they try and access someone elses account as a standard user
            if (AccessToken.UserId != UserId && AccessToken.Role != UserRole.Admin)
                return Unauthorized(value: "You are not an admin user and therefore cannot access other peoples accounts.");

            var UserAcc = await this.UserService.GetUser(new FindUserRequest { UserId = UserId }).ConfigureAwait(false);

            return UserAcc is null ? 
                   Ok(UserAcc) : 
                   NotFound($"User was not found: '{ UserId }'");

        }

        /// <summary>
        /// This is where users can register their account
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerOperation(summary: "Register a user by parsing a request to this endpoint")]
        [SwaggerResponse(StatusCodes.Status201Created, description: "The user was registered successfully")]
        [SwaggerResponse(StatusCodes.Status409Conflict, description: "The user already exists within the service")]
        public async Task<ObjectResult> Register([FromBody] CreateUserRequest Request)
        {

            // Ensure that the user doesn't already exist

            if (await this.UserService.GetUser(new FindUserRequest { Username = Request.Username }) != null)
                return StatusCode(StatusCodes.Status409Conflict, value: "User already exists.");

            var CreatedUser = await this.UserService.CreateUser(Request).ConfigureAwait(false);

            var Auth = await AuthService.CreateUserAuthentication(CreatedUser, Request.Password).ConfigureAwait(false);

            // Queue the sending of the confirmation email, but just return OK without waiting for sendgrid to respond.
            _ = EmailService.SendConfirmationEmail(CreatedUser).ConfigureAwait(false);

            return StatusCode(StatusCodes.Status201Created, value: "Created user successfully!");

        }

        /// <summary>
        /// This is where callbacks come to validate the email address.
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpGet("register/validate")]
        [AllowAnonymous]
        [SwaggerOperation(summary: "Validate a users email by parsing their UserId to this endpoint")]
        [SwaggerResponse(StatusCodes.Status202Accepted, description: "The user had their email verified successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, description: "The user Id does not exist.")]
        public async Task<StatusCodeResult> ValidateEmailOfUser([FromQuery] string UserId)
        {

            var User = await this.UserService.GetUser(new FindUserRequest { UserId = UserId }).ConfigureAwait(false);

            User.EmailVerified = true;

            // Try and validate the user email, and if it fails, return BadRequest
            return await this.UserService.UpdateUser(User)
                                         .ConfigureAwait(false)
                                         ? StatusCode(StatusCodes.Status202Accepted) : BadRequest();

        }

    }
}
