using Database.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Tokens.Models;
using Users.Interface.Models;
using Users.Interface.Models.Http;
using Users.Models;

namespace Users.Services
{
    public interface IUserService
    {

        /// <summary>
        /// Creates a user based on the criteria within the <see cref="CreateUserRequest"/>
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public Task<IGopherUser> CreateUser(CreateUserRequest Request);

        /// <summary>
        /// Retrieves a user based on 
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public Task<IGopherUser?> GetUser(FindUserRequest Filter, IAccessToken Access);

        public Task<IGopherUser?> DeleteUser(string UserId, IAccessToken Access);

        public Task<bool> UpdateUser(string UserId, IGopherUserData ModifiedData, IAccessToken Access);

    }

    public class UserService : IUserService
    {

        private readonly IDatabaseService Database;
        private readonly IMemoryCache Cache;
        private static readonly MemoryCacheEntryOptions _userModelCacheOptions = new MemoryCacheEntryOptions()
        {
            Size = 500, // I did some very basic investigation and found UserModel's usually ~272 bytes in memory. 500 is buffer.
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
        };
        private static readonly MemoryCacheEntryOptions _userModelCopyCacheOptions = new MemoryCacheEntryOptions()
        {
            Size = 0, // We store copies pointing to the same block of memory so all good size is 0
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
        };

        public UserService(IDatabaseService Database, IMemoryCache cache)
        {
            this.Database = Database;
            Cache = cache;
        }

        /// <summary>
        /// Checks that the given access token has authority to access the information of a Gopher user
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        private static bool AuthorizedAccessToUserInfo(IGopherUser User, IAccessToken Token)
        {

            // If there is no signed JWT, how can we verify the access token is valid?
            if (string.IsNullOrEmpty(Token.SignedJWT))
                return false;

            
            if (string.IsNullOrEmpty(Token.UserId) is false && string.IsNullOrEmpty(User.UserId) is false)
                return User.UserId == Token.UserId || Token.Role.Equals("System", StringComparison.OrdinalIgnoreCase);

            else if (string.IsNullOrEmpty(Token.Username) is false && string.IsNullOrEmpty(Token.Username) is false)
                return User.Username == Token.Username || Token.Role.Equals("System", StringComparison.OrdinalIgnoreCase);

            // No target of the username or userid
            else
                return false;

        }

        public async Task<IGopherUser> CreateUser(CreateUserRequest Request)
        {

            

            // Check user doesnt exist
            if (await GetUser(new FindUserRequest { Username = Request.Username }, null).ConfigureAwait(false) is not null)
                return null;

            var NewUser = new GopherUser
            {

                UserId = Guid.NewGuid().ToString(),
                Username = Request.Username,
                DisplayName = Request.DisplayName,
                DateOfBirth = Request.DateOfBirth,
                Email = Request.Email,
                Organisation = Request.OrganisationName
            };

            await Database.Collection("Users")
                               .Document(NewUser.UserId)
                               .CreateDocumentAsync(NewUser)
                               .ConfigureAwait(false);

            Cache.Set(NewUser.UserId, NewUser, _userModelCacheOptions);
            Cache.Set(NewUser.Username, NewUser, _userModelCopyCacheOptions);

            return NewUser;

        }

        public async Task<IGopherUser?> GetUser(FindUserRequest Request, IAccessToken Access)
        {

            IGopherUser? user;

            //
            // Find based on the UserId
            if (Request.UserId is not null)
            {
                if (Cache.TryGetValue(Request.UserId, out user))
                    return user;

                user = await Database.Collection("Users")
                                     .Document(Request.UserId)
                                     .GetDocumentAsync<IGopherUser>()
                                     .ConfigureAwait(false);

                if (user is null)
                    return null;

                // Cache the user for later
                Cache.Set(user.UserId, user, _userModelCacheOptions);
                Cache.Set(user.Username, user, _userModelCopyCacheOptions);

            }

            //
            // Find based on the Username
            else if (Request.Username is not null)
            {

                if (Cache.TryGetValue(Request.Username, out user))
                    return user;

                user = (await Database.Collection("Users")
                                  .WhereEqual(nameof(IGopherUser.Username), Request.Username)
                                  .Limit(1)
                                  .GetAsync<IGopherUser>().ConfigureAwait(false))
                                  .FirstOrDefault();

                if (user is null)
                    return null;

                // Cache the user for later
                Cache.Set(user.UserId, user, _userModelCacheOptions);
                Cache.Set(user.Username, user, _userModelCopyCacheOptions);
            }

            //
            // Find based on the Email
            else
            {

                user = (await Database.Collection("Users")
                                  .WhereEqual(nameof(IGopherUser.Email), Request.Email)
                                  .Limit(1)
                                  .GetAsync<IGopherUser>().ConfigureAwait(false))
                                  .FirstOrDefault();

                if (user is null)
                    return null;

                // Cache the user for later
                Cache.Set(user.UserId, user, _userModelCacheOptions);
                Cache.Set(user.Username, user, _userModelCopyCacheOptions);

            }

            // If the access token doesn't match, DO NOT hand back personal information.
            // Only show that the user exists.

            if (AuthorizedAccessToUserInfo(user, Access))
                return new GopherUser()
                {
                    UserId = user.UserId,
                    Username = user.Username,
                };

            return user;

        }

        public async Task<IGopherUser?> DeleteUser(string UserId, IAccessToken Access)
        {

            // Check that the access token has access to delete the user
            if (AuthorizedAccessToUserInfo(new GopherUser
            {
                UserId = UserId,
            }, Access) is false)
                return null;

            return await Database.Collection("Users")
                                      .Document(UserId)
                                      .DeleteDocumentAsync<IGopherUser>()
                                      .ConfigureAwait(false);

        }

        public async Task<bool> UpdateUser(string UserId, IGopherUserData Request, IAccessToken Access)
        {

            var OldUserData = await GetUser(new FindUserRequest { UserId = UserId }, Access).ConfigureAwait(false);

            if (AuthorizedAccessToUserInfo(OldUserData, Access) is false)
                return false;

            if (OldUserData is null)
                return false;

            var NewUserData = new GopherUser
            {

                UserId = OldUserData.UserId,
                Username = OldUserData.Username,
                Email = Request.Email ?? OldUserData.Email,
                EmailVerified = Request.EmailVerified ?? Request.Email is null ? OldUserData.EmailVerified : false,
                Created = OldUserData.Created,
                DateOfBirth = Request.DateOfBirth == default ? OldUserData.DateOfBirth : Request.DateOfBirth,
                Organisation = Request.Organisation ?? OldUserData.Organisation
            };

            Cache.Set(NewUserData.UserId, NewUserData, _userModelCacheOptions);
            Cache.Set(NewUserData.Username, NewUserData, _userModelCopyCacheOptions);

            return await Database.Collection("Users")
                                       .Document(NewUserData.UserId)
                                       .SetDocumentAsync(NewUserData)
                                       .ConfigureAwait(false) is not null;

        }



    }

}
