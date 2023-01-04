using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Users.Interface.Models;

namespace Users.Interface
{

    /// <summary>
    /// An interaction point to talk to the Users API
    /// </summary>
    public interface IUsersAPI
    {

        /// <summary>
        /// Register a new Gopher Industries user
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Email"></param>
        /// <param name="DateOfBirth"></param>
        /// <param name="OrganisationName"></param>
        /// <returns></returns>
        public Task<bool> RegisterUser(string Username, string Password, string Email, DateTime DateOfBirth, string OrganisationName);

        /// <summary>
        /// Retrieve a user's information
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="AccessToken"></param>
        /// <returns></returns>
        public Task<IGopherUser?> GetUser(string UserId, string AccessToken);

        /// <summary>
        /// Update a user's information with the values specified in the <paramref name="ChangedData"/>. <br/> 
        /// Fill in values you wish to update, and leave blank those that you don't want to change.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="ChangedData">The user data to update. Fill in values you wish to update, and leave blank those that you don't want to change.</param>
        /// <param name="AccessToken"></param>
        /// <returns></returns>
        public Task<IGopherUser?> UpdateUser(string UserId, IGopherUserData ChangedData, string AccessToken);

        /// <summary>
        /// Delete a user and all of their information
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="AccessToken"></param>
        /// <returns></returns>
        public Task<bool> DeleteUser(string UserId, string AccessToken);

    }

    public class UsersAPI : IUsersAPI
    {

        private readonly string targetEndpoint;
        private readonly IHttpClientFactory httpFactory;

        public UsersAPI()
        {
            this.targetEndpoint = this.GetEndpointFromEnvironment();
            this.httpFactory = new DefaultHttpFactory();
        }

        public UsersAPI(IHttpClientFactory httpFactory)
        {
            this.targetEndpoint = this.GetEndpointFromEnvironment();
            this.httpFactory = httpFactory;
        }

        public async Task<bool> RegisterUser(string Username, string Password, string Email, DateTime DateOfBirth, string OrganisationName)
        {

            var client = this.httpFactory.CreateClient();

            var serverResponse = await client.PostAsync(this.targetEndpoint, JsonContent.Create(new
            {
                Username = Username,
                Password = Password,
                Email = Email,
                DateOfBirth = DateOfBirth,
                OrganisationName = OrganisationName
            })).ConfigureAwait(false);

            return serverResponse.IsSuccessStatusCode;

        }

        public async Task<IGopherUser?> GetUser(string UserId, string AccessToken)
        {

            var client = this.httpFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var serverResponse = await client.GetAsync(this.targetEndpoint + $"?UserId={ UserId }");

            if (serverResponse.IsSuccessStatusCode is false)
                return null;

            return JsonSerializer.Deserialize<IGopherUser>(await serverResponse.Content.ReadAsStringAsync());

        }

        public async Task<IGopherUser?> UpdateUser(string UserId, IGopherUserData ChangedData, string AccessToken)
        {

            var client = this.httpFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var serverResponse = await client.PatchAsync(this.targetEndpoint + $"?UserId={UserId}", JsonContent.Create(ChangedData));

            if (serverResponse.IsSuccessStatusCode is false)
                return null;

            return JsonSerializer.Deserialize<IGopherUser>(await serverResponse.Content.ReadAsStringAsync());

        }

        public async Task<bool> DeleteUser(string UserId, string AccessToken)
        {

            var client = this.httpFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var serverResponse = await client.DeleteAsync(this.targetEndpoint + $"?UserId={UserId}");

            return serverResponse.IsSuccessStatusCode;
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
