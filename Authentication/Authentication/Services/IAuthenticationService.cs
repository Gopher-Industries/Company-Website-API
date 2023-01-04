using Authentication.Interface;
using Authentication.Interface.Models.Rest.Response;
using Authentication.Models.Tokens;
using Authentication.Models.Users;
using Database.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using Users.Interface;

namespace Authentication.Services
{

    public interface IAuthenticationService : IAuthenticationAPI
    {

        /// <summary>
        /// Tries to retrieve the user authentication from the database using the user's Id.
        /// </summary>
        /// <returns></returns>
        public Task<UserAuthenticationModel?> GetUserAuthenticationByUserId(string UserId);

        /// <summary>
        /// Tries to retrieve the user authentication from the database using the user's username.
        /// </summary>
        /// <returns></returns>
        public Task<UserAuthenticationModel?> GetUserAuthenticationByUsername(string Username);

    }

    public class BCryptAuthenticationService : IAuthenticationService
    {

        private readonly IDatabaseService database;
        private readonly ITokenService tokenService;
        private readonly IUsersAPI usersService;
        private readonly IMemoryCache cache;
        private static readonly MemoryCacheEntryOptions _userAuthModelCacheOptions = new MemoryCacheEntryOptions()
        {
            Size = 400, // I did some very basic investigation and found UserModel's usually ~350 bytes in memory. 400 is buffer.
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        };

        public BCryptAuthenticationService(IDatabaseService database, ITokenService tokenService, IMemoryCache cache, IUsersAPI usersService)
        {
            this.database = database;
            this.tokenService = tokenService;
            this.cache = cache;
            this.usersService = usersService;
        }

        #region Interface Exposed Functions



        public async Task<LoginResponse?> CreateUserAuthentication(string UserId, string PlainTextPassword)
        {

            //
            // If there's already a user authentication model
            if ((await this.database.GetDocument<UserAuthenticationModel>("UsersAuthentication", UserId).ConfigureAwait(false)) is not null)
                return null;

            //
            // Talk to the Users Microservice API to check that the person is registered with them.
            var TokenPair = tokenService.GenerateNewTokenPair(UserId, "Empty for now", "Empty for now");

            var User = await usersService.GetUser(UserId, TokenPair.AccessToken.SignedJWT).ConfigureAwait(false);

            // If the user does not exist in the Users Microservice, then we don't create them.
            if (User is null)
                return null;

            var UserAuth = new UserAuthenticationModel()
            {
                UserId = UserId,
                Username = User.Username,
                Salt = Encoding.ASCII.GetString(RandomNumberGenerator.GetBytes(20)),
                Pepper = Encoding.ASCII.GetString(RandomNumberGenerator.GetBytes(20))
            };

            UserAuth.HashedPassword = BCrypt.Net.BCrypt.HashPassword(
                inputKey: UserAuth.Salt + PlainTextPassword + UserAuth.Pepper,
                salt: BCrypt.Net.BCrypt.GenerateSalt(workFactor: 10), // Work factor: Between 1 and 31
                enhancedEntropy: true,
                hashType: BCrypt.Net.HashType.SHA512);

            var CreateRequest = await this.database.SetDocument(
                                                        CollectionPath: "UsersAuthentication",
                                                        DocumentId: User.Username,
                                                        Value: UserAuth)
                                                    .ConfigureAwait(false);

            // Add authentication model to cache for quick retrieval
            cache.Set("UserId-" + UserId, UserAuth, _userAuthModelCacheOptions);
            cache.Set("Username-" + User.Username, UserAuth, _userAuthModelCacheOptions);



            return new LoginResponse()
            {
                AccessToken = TokenPair.AccessToken.SignedJWT,
                RefreshToken = TokenPair.RefreshToken.SignedJWT
            };

        }

        public async Task<LoginResponse?> Login(string Username, string PlainTextPassword)
        {

            var UserAuth = await this.GetUserAuthenticationByUsername(Username).ConfigureAwait(false);

            // Check that the password matches
            if (this.MatchingPassword(PlainTextPassword, UserAuth) is false)
                return null;

            var TokenPair = this.tokenService.GenerateNewTokenPair(UserAuth.UserId, UserAuth.Username, "Empty for now");

            return new LoginResponse
            {
                AccessToken = TokenPair.AccessToken.SignedJWT,
                RefreshToken = TokenPair.RefreshToken.SignedJWT
            };

        }

        public async Task<LoginResponse?> RefreshLogin(string RefreshJWT)
        {

            // Validate that the refresh token is what we expect
            if (this.tokenService.ValidateRefreshToken(RefreshJWT) is false)
                return null;

            var RefreshToken = this.tokenService.ReadRefreshToken(RefreshJWT);

            if (RefreshToken is null)
                return null;

            var TokenPair = this.tokenService.GenerateNewTokenPair(RefreshToken);

            return new LoginResponse()
            {
                AccessToken = TokenPair.AccessToken.SignedJWT,
                RefreshToken = TokenPair.RefreshToken.SignedJWT
            };

        }

        public async Task<UserAuthenticationModel?> GetUserAuthenticationByUserId(string UserId)
        {

            //
            // Check if we have the auth cached
            if (cache.TryGetValue("UserId-" + UserId, out UserAuthenticationModel? UserAuth))
                return UserAuth;

            // Retrieve the users authentication from the database

            UserAuth = await database.GetDocument<UserAuthenticationModel>(
                                        CollectionPath: "UsersAuthentication",
                                        DocumentId: UserId)
                                     .ConfigureAwait(false);

            if (UserAuth is null)
                return null;

            cache.Set("UserId-" + UserAuth.UserId, UserAuth, _userAuthModelCacheOptions);
            cache.Set("Username-" + UserAuth.Username, UserAuth, _userAuthModelCacheOptions);

            return UserAuth;

        }

        public async Task<UserAuthenticationModel?> GetUserAuthenticationByUsername(string Username)
        {

            //
            // Check if we have the auth cached
            if (cache.TryGetValue("Username-" + Username, out UserAuthenticationModel? UserAuth))
                return UserAuth;

            // Retrieve the users authentication from the database

            UserAuth = await database.GetDocument<UserAuthenticationModel>(
                                        CollectionPath: "UsersAuthentication",
                                        DocumentId: Username)
                                     .ConfigureAwait(false);

            if (UserAuth is null)
                return null;

            cache.Set("UserId-" + UserAuth.UserId, UserAuth, _userAuthModelCacheOptions);
            cache.Set("Username-" + Username, UserAuth, _userAuthModelCacheOptions);

            return UserAuth;

        }

        #endregion


        #region Interface HiddenFunctions



        private async Task<RefreshTokenDatabaseEntry?> GetRefreshToken(string UserId, string TokenId)
        {

            // Instruct database to delete any docs that aren't valid
            await this.ValidateUserRefreshTokens(UserId).ConfigureAwait(false);

            //
            // Find the refresh token and return it
            //return (await Database.Collection("UsersAuthentication")
            //                      .Document(User.UserId)
            //                      .Collection("RefreshTokens")
            //                      .Document(TokenId)
            //                      .GetSnapshotAsync()
            //                      .ConfigureAwait(false))?
            //                      .ConvertTo<RefreshTokenDatabaseEntry?>();
            return (await this.database.GetDocument<RefreshTokenDatabaseEntry>(
                                                   CollectionPath: $"UsersAuthentication/{UserId}/RefreshTokens", TokenId));

        }

        private async Task<IReadOnlyList<RefreshTokenDatabaseEntry>> ValidateUserRefreshTokens(string UserId)
        {

            // Instruct database to delete any docs that aren't valid
            var ExpiredDocs = await this.database.Collection("UsersAuthentication")
                                                 .Document(UserId)
                                                 .Collection("RefreshTokens")
                                                 .WhereLessThanOrEqual(nameof(RefreshTokenStateful.ValidUntil), DateTime.UtcNow)
                                                 .DeleteAsync<RefreshTokenDatabaseEntry>();

            // Return all the deleted tokens
            return ExpiredDocs;

        }

        private async Task<bool> AddRefreshToken(string UserId, RefreshTokenStateful RefreshToken)
        {

            // Because google is smart, it sees through our polymorphism and sees that the RefreshTokenDatabaseEntry
            // is actually RefreshToken. Therefore, we just shallow copy the data over to a real instance
            // of RefreshTokenDatabaseEntry lol.

            var DbEntry = new RefreshTokenDatabaseEntry
            {
                Secret = RefreshToken.Secret,
                TokenId = RefreshToken.TokenId,
                ValidUntil = RefreshToken.ValidUntil
            };

            return (await database.Collection("UsersAuthentication")
                                  .Document(UserId)
                                  .Collection("RefreshTokens")
                                  .Document(RefreshToken.TokenId)
                                  .SetDocumentAsync(DbEntry)
                                  .ConfigureAwait(false)) is not null;

        }

        /// <summary>
        /// Checks that the password matches the hashed password from the database using the BCrypt algorithm.
        /// </summary>
        /// <param name="PlainTextPassword"></param>
        /// <param name="AuthModel"></param>
        /// <returns></returns>
        private bool MatchingPassword(string PlainTextPassword, UserAuthenticationModel? AuthModel)
        {

            if (AuthModel == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(
                text: AuthModel.Salt + PlainTextPassword + AuthModel.Pepper,
                hash: AuthModel.HashedPassword,
                enhancedEntropy: true,
                hashType: BCrypt.Net.HashType.SHA512);

        }

        #endregion

    }

}
