using Authentication.Interface.Models.Rest.Response;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Authentication.Interface
{

    /// <summary>
    /// The API responsible for authenticating user logins.
    /// </summary>
    public interface IAuthenticationAPI
    {

        /// <summary>
        /// Creates an authentication model using a plain text password. Stores it in the database. <br/>
        /// FOR INTERNAL USE ONLY. Is called by the users microservice when creating a new user. <br/>
        /// User must exist in users microservice and not have an existing authentication context.
        /// </summary>
        /// <param name="PlainTextPassword"></param>
        /// <returns></returns>
        public Task<LoginResponse?> CreateUserAuthentication(string UserId, string PlainTextPassword);

        /// <summary>
        /// Tries to login as the user using the user's plain text password.
        /// </summary>
        /// <param name="Username">The user's unique id.</param>
        /// <param name="PlainTextPassword">The password in plain text form for the given user.</param>
        /// <returns>A model containing both an access token and a refresh token to use later. If login was unsucessful, returns null.</returns>
        public Task<LoginResponse?> Login(string Username, string PlainTextPassword);

        /// <summary>
        /// Tries to refresh the login credentials using the given refresh token. <br/>
        /// The <paramref name="RefreshJWT"/> will be replaced by the new refresh token in <see cref="LoginResponse"/>
        /// </summary>
        /// <param name="RefreshJWT">The refresh token given</param>
        /// <returns>A model containing both an access token and a refresh token to use later. If refresh token is invalid or expired, returns null.</returns>
        public Task<LoginResponse?> RefreshLogin(string RefreshJWT);

    }

    public class AuthenticationAPI : IAuthenticationAPI
    {

        private readonly string targetEndpoint;
        private readonly IHttpClientFactory httpFactory;

        public AuthenticationAPI()
        {
            this.targetEndpoint = this.GetEndpointFromEnvironment();
            this.httpFactory = new DefaultHttpFactory();
        }

        public AuthenticationAPI(IHttpClientFactory httpFactory)
        {
            this.targetEndpoint = this.GetEndpointFromEnvironment();
            this.httpFactory = httpFactory;
        }

        public async Task<LoginResponse?> CreateUserAuthentication(string UserId, string PlainTextPassword)
        {

            var client = this.httpFactory.CreateClient();

            var serverResponse = await client.PostAsync(this.targetEndpoint, JsonContent.Create(new
            {
                UserId = UserId,
                PlainTextPassword = PlainTextPassword
            })).ConfigureAwait(false);

            return JsonSerializer.Deserialize<LoginResponse>(await serverResponse.Content.ReadAsStringAsync());

        }

        public async Task<LoginResponse?> Login(string Username, string PlainTextPassword)
        {

            var client = this.httpFactory.CreateClient();

            var serverResponse = await client.PostAsync(this.targetEndpoint, JsonContent.Create(new
            {
                Username = Username,
                PlainTextPassword = PlainTextPassword
            })).ConfigureAwait(false);

            return JsonSerializer.Deserialize<LoginResponse>(await serverResponse.Content.ReadAsStringAsync());

        }

        public async Task<LoginResponse?> RefreshLogin(string RefreshJWT)
        {

            var client = this.httpFactory.CreateClient();

            var serverResponse = await client.PostAsync(this.targetEndpoint, JsonContent.Create(new
            {
                RefreshJWT = RefreshJWT
            })).ConfigureAwait(false);

            return JsonSerializer.Deserialize<LoginResponse>(await serverResponse.Content.ReadAsStringAsync());

        }

        private string GetEndpointFromEnvironment()
        {

            var EnvironmentType = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return EnvironmentType?.ToLower() switch
            {
                "development" => "dev.api.gopherindustries.net",
                "staging" => "stg.api.gopherindustries.net",
                "production" => "api.gopherindustries.net",
                _ => throw new KeyNotFoundException("Environment variable 'ASPNETCORE_ENVIRONMENT' expected. Valid values: [ 'Development', 'Staging', 'Production' ]")
            };

        }

        private class DefaultHttpFactory : IHttpClientFactory
        {
            public HttpClient CreateClient(string name)
            {
                return new HttpClient();
            }
        }

    }

}
