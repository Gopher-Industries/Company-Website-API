using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Users.Models;
using Users.Services;
using Tokens;
using Authentication.Interface;
using Users.Interface.Models.Http;
using Users.Interface.Models;
using Authentication.Interface.Models.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Users.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {

        private readonly IUserService UserService;
        private readonly IEmailService EmailService;
        private readonly IAuthenticationAPI AuthService;
        private readonly ITokenValidationService TokenService;

        public UsersController(IUserService UserService,
                               ITokenValidationService TokenService,
                               IEmailService EmailService,
                               IAuthenticationAPI AuthService)
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
        public async Task<ObjectResult> GetUser([FromRoute] string UserId)
        {

            var AccessToken = TokenService.ReadAccessToken(HttpContext.User);

            // If they try and access someone elses account as a standard user
            if (AccessToken?.UserId != UserId &&
                AccessToken?.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) is false)
                return Unauthorized(value: "You are not an admin user and therefore cannot access other peoples accounts.");

            var UserAcc = await UserService.GetUser(new FindUserRequest { UserId = UserId }, AccessToken).ConfigureAwait(false);

            return UserAcc is not null ?
                   Ok(UserAcc) :
                   NotFound($"User was not found: '{UserId}'");

        }

        /// <summary>
        /// Retrieve information regarding a user
        /// </summary>
        /// <param name="Criteria"></param>
        /// <returns></returns>
        [HttpGet("search", Name = "Find User")]
        public async Task<ObjectResult> FindUser([FromQuery] FindUserRequest Criteria)
        {

            var AccessToken = TokenService.ReadAccessToken(HttpContext.User);

            // If they try and access someone elses account as a standard user
            if (AccessToken?.UserId != Criteria.UserId &&
                AccessToken?.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) is false &&
                AccessToken?.Role.Equals("System", StringComparison.OrdinalIgnoreCase) is false)
                return Unauthorized(value: "You are not an admin user and therefore cannot access other peoples accounts.");

            var UserAcc = await UserService.GetUser(Criteria, AccessToken).ConfigureAwait(false);

            return UserAcc is not null ?
                   Ok(UserAcc) :
                   NotFound($"User matching search was not found");

        }

        /// <summary>
        /// This is where users can register their account
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost()]
        [AllowAnonymous]
        [SwaggerOperation(summary: "Register a user by parsing a request to this endpoint")]
        [SwaggerResponse(StatusCodes.Status201Created, description: "The user was registered successfully")]
        [SwaggerResponse(StatusCodes.Status409Conflict, description: "The user already exists within the service")]
        public async Task<ObjectResult> CreateUser([FromBody] CreateUserRequest Request)
        {

            // Ensure that the user doesn't already exist

            if (await UserService.GetUser(new FindUserRequest { Username = Request.Username }, default) != null)
                return StatusCode(StatusCodes.Status409Conflict, value: "User already exists.");

            var CreatedUser = await UserService.CreateUser(Request).ConfigureAwait(false);

            var Auth = await AuthService.CreateUserAuthentication(CreatedUser.UserId, Request.Password).ConfigureAwait(false);

            // Queue the sending of the confirmation email, but just return OK without waiting for sendgrid to respond.
            _ = EmailService.SendConfirmationEmail(CreatedUser).ConfigureAwait(false);

            return StatusCode(StatusCodes.Status201Created, value: "Created user successfully!");

        }

        /// <summary>
        /// Retrieve information regarding a user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpDelete("{UserId}", Name = "Delete User")]
        public async Task<ObjectResult> DeleteUser([FromRoute] string UserId)
        {

            var AccessToken = TokenService.ReadAccessToken(HttpContext.User);

            // If they try and access someone elses account as a standard user
            if (AccessToken.UserId != UserId && AccessToken.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(value: "You are not an admin user and therefore cannot access other peoples accounts.");

            var DeletedUserAccount = await UserService.DeleteUser(UserId, AccessToken).ConfigureAwait(false);

            return DeletedUserAccount is not null ?
                   Ok(DeletedUserAccount) :
                   NotFound($"User was not found: '{UserId}'");

        }

        /// <summary>
        /// Retrieve information regarding a user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpPatch("{UserId}", Name = "Edit User")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The user had their email verified successfully.")]
        public async Task<ObjectResult> UpdateUser([FromRoute] string UserId, [FromBody] IGopherUserData Request)
        {

            var AccessToken = TokenService.ReadAccessToken(HttpContext.User);

            // If they try and access someone elses account as a standard user
            if (AccessToken.UserId != UserId && AccessToken.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(value: "You are not an admin user and therefore cannot access other peoples accounts.");

            var UpdatedUser = await UserService.UpdateUser(UserId, Request, AccessToken).ConfigureAwait(false);

            return UpdatedUser ?
                   Ok(UpdatedUser) :
                   NotFound($"User was not found: '{UserId}'");

        }

        /// <summary>
        /// This is where callbacks come to validate the email address.
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpGet("{UserId}/validate")]
        [AllowAnonymous]
        [SwaggerOperation(summary: "Validate a users email by parsing their UserId to this endpoint")]
        [SwaggerResponse(StatusCodes.Status202Accepted, description: "The user had their email verified successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, description: "The user Id does not exist.")]
        public async Task<StatusCodeResult> ValidateEmailOfUser([FromRoute] string UserId)
        {

            var AccessToken = TokenService.ReadAccessToken(HttpContext.User);

            // Try and validate the user email, and if it fails, return BadRequest
            return await UserService.UpdateUser(UserId, new GopherUserData
                                                {
                                                    EmailVerified = true
                                                }, AccessToken)
                                                .ConfigureAwait(false) ?
                                                 StatusCode(StatusCodes.Status202Accepted) :
                                                 BadRequest();

        }

    }
}
