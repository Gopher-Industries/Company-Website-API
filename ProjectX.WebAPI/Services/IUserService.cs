using Google.Cloud.Firestore;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Caching.Memory;
using ProjectX.WebAPI.Models.Database.Authentication;
using ProjectX.WebAPI.Models.Database.Timeline;
using ProjectX.WebAPI.Models.RestRequests.Request;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types.FieldFilter.Types;

namespace ProjectX.WebAPI.Services
{
    public interface IUserService
    {

        public Task<UserModel> CreateUser(CreateUserRequest Request);

        public Task<UserModel> GetUser(FindUserRequest Filter);

        public Task<bool> UpdateUser(UserModel User);

    }

    public class UserService : IUserService
    {

        private readonly IDatabaseService Database;
        private readonly IMemoryCache Cache;
        private readonly IDatabaseService DebugDatabase;
        private readonly MemoryCacheEntryOptions _userModelCacheOptions = new MemoryCacheEntryOptions()
        {
            Size = 500, // I did some very basic investigation and found UserModel's usually ~272 bytes in memory. 500 is buffer.
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
        };
        private readonly MemoryCacheEntryOptions _userModelCopyCacheOptions = new MemoryCacheEntryOptions()
        {
            Size = 0, // We store copies pointing to the same block of memory so all good size is 0
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
        };

        public UserService(IDatabaseService firestore, IMemoryCache cache, IDatabaseService debugDatabase)
        {
            Database = firestore;
            Cache = cache;
            DebugDatabase = debugDatabase;
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
            if (Request.UserId != null)
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
            else if (Request.Username != null)
            {

                if (Cache.TryGetValue(Request.Username, out user))
                    return user;

                user = (await Database.Collection("Users")
                                  .WhereEqual("Username", Request.Username)
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
                                  .WhereEqual("Email", Request.Email)
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

        public async Task<bool> UpdateUser(UserModel User)
        {

            //return this.DebugDatabase.SetDocument("Users", User.UserId, User);

            //var StudentDoc = await this.DebugDatabase.GetDocument<TeamStudent>("Timeline/Collections/Students", "e386bc95-bf6e-42c8-bcde-79b50fcf41c1");
            //var StudentsDoc = await this.DebugDatabase.GetDocuments<TeamStudent>("Timeline/Collections/Students",
            //                                                                     DatabaseFilter.Equal(FieldPath.DocumentId.ToString(), "e386bc95-bf6e-42c8-bcde-79b50fcf41c1"));

            return (await this.Database.Collection("Users")
                                       .Document(User.UserId)
                                       .SetDocumentAsync(User)
                                       .ConfigureAwait(false)) is not null;

        }

        

    }

}
