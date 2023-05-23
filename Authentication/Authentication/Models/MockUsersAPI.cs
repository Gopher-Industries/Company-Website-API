using Users.Interface;
using Users.Interface.Models;

namespace Authentication.Models
{
    public class MockUsersAPI : IUsersAPI
    {

        private class MockGopherUser : IGopherUser
        {
            public string UserId { get; init; }
            public string Username { get; init; }
            public DateTime Created { get; init; }
            public string DisplayName { get; init; }
            public string Email { get; init; }
            public string Organisation { get; init; }
            public bool? EmailVerified { get; init; }
            public DateTime DateOfBirth { get; init; }
        }

        public async Task<bool> DeleteUser(string UserId, string AccessToken)
        {
            return false;
        }

        public async Task<IGopherUser?> GetUser(string UserId, string AccessToken)
        {
            return new MockGopherUser
            {
                UserId = UserId,
                Created = DateTime.Now,
                DateOfBirth = DateTime.Now,
                DisplayName = "Default User",
                Email = "default@gopherindustries.net",
                EmailVerified = false,
                Organisation = "aaaaaaaaaaaaaaa idk",
                Username = "defaultuser"
            };
        }

        public async Task<bool> RegisterUser(string Username, string Password, string Email, DateTime DateOfBirth, string OrganisationName)
        {
            return true;
        }

        public async Task<IGopherUser?> UpdateUser(string UserId, IGopherUserData ChangedData, string AccessToken)
        {
            return new MockGopherUser
            {
                UserId = UserId,
                Created = DateTime.Now,
                DateOfBirth = DateTime.Now,
                DisplayName = "Default User",
                Email = "default@gopherindustries.net",
                EmailVerified = false,
                Organisation = "aaaaaaaaaaaaaaa idk",
                Username = "defaultuser"
            };
        }
    }
}
