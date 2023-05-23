using Microsoft.Extensions.Caching.Memory;
using ProjectX.WebAPI.Models.Database.Authentication;
using ProjectX.WebAPI.Models.RestRequests.Request.Users;

namespace ProjectX.WebAPI.Services
{
    public interface IUserService
    {

        /// <summary>
        /// Creates a user based on the criteria within the <see cref="CreateUserRequest"/>
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public Task<UserModel> CreateUser(CreateUserRequest Request);

        /// <summary>
        /// Retrieves a user based on 
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public Task<UserModel?> GetUser(FindUserRequest Filter);

        public Task<UserModel?> DeleteUser(string UserId);

        public Task<bool> UpdateUser(UpdateUserRequest Request);

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

        public async Task<UserModel?> CreateUser(CreateUserRequest Request)
        {

            // Check user doesnt exist
            if (await this.GetUser(new FindUserRequest { Username = Request.Username }).ConfigureAwait(false) is not null)
                return null;

            var NewUser = new UserModel(

                UserId: Guid.NewGuid().ToString(),
                Username: Request.Username,
                DateOfBirth: Request.DateOfBirth,
                Email: Request.Email,
                Organisation: Request.OrganisationName
            );

            await this.Database.Collection("Users")
                               .Document(NewUser.UserId)
                               .CreateDocumentAsync(NewUser)
                               .ConfigureAwait(false);

            Cache.Set(NewUser.UserId, NewUser, _userModelCacheOptions);
            Cache.Set(NewUser.Username, NewUser, _userModelCopyCacheOptions);

            return NewUser;

        }

        public async Task<UserModel?> GetUser(FindUserRequest Request)
        {

            UserModel? user;

            //
            // Find based on the UserId
            if (Request.UserId is not null)
            {
                if (Cache.TryGetValue(Request.UserId, out user))
                    return user;

                user = await Database.Collection("Users")
                                     .Document(Request.UserId)
                                     .GetDocumentAsync<UserModel>()
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
                                  .WhereEqual(nameof(UserModel.Username), Request.Username)
                                  .Limit(1)
                                  .GetAsync<UserModel>().ConfigureAwait(false))
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
                                  .WhereEqual(nameof(UserModel.Email), Request.Email)
                                  .Limit(1)
                                  .GetAsync<UserModel>().ConfigureAwait(false))
                                  .FirstOrDefault();

                if (user is null)
                    return null;

                // Cache the user for later
                Cache.Set(user.UserId, user, _userModelCacheOptions);
                Cache.Set(user.Username, user, _userModelCopyCacheOptions);

            }

            return user;

        }

        public async Task<UserModel?> DeleteUser(string UserId)
        {

            await this.Database.Collection("UsersAuthentication")
                               .Document(UserId)
                               .DeleteDocumentAsync<UserAuthenticationModel>()
                               .ConfigureAwait(false);

            return await this.Database.Collection("Users")
                                      .Document(UserId)
                                      .DeleteDocumentAsync<UserModel>()
                                      .ConfigureAwait(false);

        }

        public async Task<bool> UpdateUser(UpdateUserRequest Request)
        {

            var OldUserData = await this.GetUser(new FindUserRequest { UserId = Request.UserId }).ConfigureAwait(false);
            
            if (OldUserData is null)
                return false;

            if (Request.Email is not null)
                Request.EmailVerified = false;

            var NewUserData = new UserModel
            {
                UserId = OldUserData.UserId,
                Username = Request.Username ?? OldUserData.Username,
                Email = Request.Email ?? OldUserData.Email,
                EmailVerified = Request.EmailVerified ?? OldUserData.EmailVerified,
                Created = OldUserData.Created,
                DateOfBirth = Request.DateOfBirth ?? OldUserData.DateOfBirth,
                Organisation = Request.OrganisationName ?? OldUserData.Organisation
            };

            Cache.Set(NewUserData.UserId, NewUserData, _userModelCacheOptions);
            Cache.Set(NewUserData.Username, NewUserData, _userModelCopyCacheOptions);

            return (await this.Database.Collection("Users")
                                       .Document(NewUserData.UserId)
                                       .SetDocumentAsync(NewUserData)
                                       .ConfigureAwait(false)) is not null;

        }

        

    }

}
