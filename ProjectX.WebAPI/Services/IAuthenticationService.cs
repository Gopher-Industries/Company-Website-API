using Google.Cloud.Firestore;
using Google.LongRunning;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using ProjectX.WebAPI.Models.Database.Authentication;
using ProjectX.WebAPI.Models.RestRequests.Request;
using System.Security.Cryptography;
using System.Text;

namespace ProjectX.WebAPI.Services
{

    public interface IAuthenticationService
    {

        /// <summary>
        /// Creates an authentication model using a plain text password. Stores it in the database.
        /// </summary>
        /// <param name="PlainTextPassword"></param>
        /// <returns></returns>
        public Task<UserAuthenticationModel> CreateUserAuthentication(UserModel User, string PlainTextPassword);

        /// <summary>
        /// Tries to retrieve the user authentication from the database.
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public Task<UserAuthenticationModel?> GetUserAuthentication(UserModel User);

        /// <summary>
        /// Tries to login as the user using the plain text password.
        /// </summary>
        /// <param name="User"></param>
        /// <param name="PlainTextPassword"></param>
        /// <returns></returns>
        public Task<UserAuthenticationModel?> TryLogin(UserModel User, string PlainTextPassword);

        public Task<RefreshTokenDatabaseEntry?> GetRefreshToken(UserModel User, string TokenId);

        /// <summary>
        /// Update a refresh token pertaining to a user
        /// </summary>
        /// <param name="User"></param>
        /// <param name="RefreshToken"></param>
        /// <returns></returns>
        public Task<bool> AddRefreshToken(UserModel User, RefreshToken RefreshToken);

        /// <summary>
        /// Check if the user has the minimum authorization level 
        /// in order to do things like access an api service
        /// </summary>
        /// <param name="AuthModel"></param>
        /// <param name="MinimumAuthorization"></param>
        /// <returns></returns>
        public bool HasPermission(UserAuthenticationModel AuthModel, UserRole MinimumAuthorization);

    }

    public class BCryptAuthenticationService : IAuthenticationService
    {

        private readonly IDatabaseService Database;
        private readonly IMemoryCache cache;
        private readonly MemoryCacheEntryOptions _userAuthModelCacheOptions = new MemoryCacheEntryOptions()
        {
            Size = 400, // I did some very basic investigation and found UserModel's usually ~350 bytes in memory. 400 is buffer.
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(6)
        };

        public BCryptAuthenticationService(IDatabaseService database, IMemoryCache cache)
        {
            Database = database;
            this.cache = cache;
        }

        

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

        public bool HasPermission(UserAuthenticationModel AuthModel, UserRole MinimumAuthorization)
        {

            if (MinimumAuthorization == UserRole.Patient)
                return true;

            if (MinimumAuthorization == UserRole.Caregiver)
                return AuthModel.Role != UserRole.Patient;

            if (MinimumAuthorization == UserRole.Admin)
                return AuthModel.Role == UserRole.Admin;

            throw new NotImplementedException("An error with reading permissions has occured.");

        }

        #region Database Interaction

        public async Task<UserAuthenticationModel> CreateUserAuthentication(UserModel User, string PlainTextPassword)
        {

            var UserAuth = new UserAuthenticationModel()
            {
                UserId = User.UserId,
                Salt = Encoding.ASCII.GetString(RandomNumberGenerator.GetBytes(20)),
                Pepper = Encoding.ASCII.GetString(RandomNumberGenerator.GetBytes(20))
            };

            UserAuth.HashedPassword = BCrypt.Net.BCrypt.HashPassword(
                inputKey: UserAuth.Salt + PlainTextPassword + UserAuth.Pepper,
                salt: BCrypt.Net.BCrypt.GenerateSalt(workFactor: 10), // Work factor: Between 1 and 31
                enhancedEntropy: true,
                hashType: BCrypt.Net.HashType.SHA512);

            var CreateRequest = await this.Database.SetDocument(
                                                        CollectionPath: "UsersAuthentication",
                                                        DocumentId: User.UserId,
                                                        Value: User)
                                                   .ConfigureAwait(false);

            // Add to cache
            cache.Set(User.UserId + "-Auth", UserAuth, _userAuthModelCacheOptions);

            return UserAuth;

        }

        public async Task<UserAuthenticationModel?> GetUserAuthentication(UserModel User)
        {

            //
            // Check if we have the auth cached
            if (cache.TryGetValue(User.UserId + "-Auth", out UserAuthenticationModel UserAuth))
                return UserAuth;

            // Retrieve the users authentication from the database
            
            UserAuth = await Database.GetDocument<UserAuthenticationModel>(
                                        CollectionPath: "UsersAuthentication",
                                        DocumentId: User.UserId)
                                     .ConfigureAwait(false);

            cache.Set(User.UserId + "-Auth", UserAuth, _userAuthModelCacheOptions);

            return UserAuth;

        }

        public async Task<RefreshTokenDatabaseEntry?> GetRefreshToken(UserModel User, string TokenId)
        {

            // Instruct database to delete any docs that aren't valid
            await this.ValidateUserRefreshTokens(User).ConfigureAwait(false);

            //
            // Find the refresh token and return it
            //return (await Database.Collection("UsersAuthentication")
            //                      .Document(User.UserId)
            //                      .Collection("RefreshTokens")
            //                      .Document(TokenId)
            //                      .GetSnapshotAsync()
            //                      .ConfigureAwait(false))?
            //                      .ConvertTo<RefreshTokenDatabaseEntry?>();
            return (await this.Database.GetDocument<RefreshTokenDatabaseEntry>(
                                                   CollectionPath: $"UsersAuthentication/{User.UserId}/RefreshTokens", TokenId));

        }

        public async Task<IReadOnlyList<RefreshTokenDatabaseEntry>> ValidateUserRefreshTokens(UserModel User)
        {

            // Instruct database to delete any docs that aren't valid
            //var ExpieredDocs = (await Database.Collection("UsersAuthentication")
            //                                  .Document(User.UserId)
            //                                  .Collection("RefreshTokens")
            //                                  .WhereLessThanOrEqualTo("ValidUntil", DateTime.UtcNow)
            //                                  .GetSnapshotAsync()
            //                                  .ConfigureAwait(false));
            var ExpiredDocs = await this.Database.Collection("UsersAuthentication")
                                                 .Document(User.UserId)
                                                 .Collection("RefreshTokens")
                                                 .WhereLessThanOrEqual(nameof(RefreshToken.ValidUntil), DateTime.UtcNow)
                                                 .DeleteAsync<RefreshTokenDatabaseEntry>();

            // Return all the deleted tokens
            return ExpiredDocs;

        }

        public async Task<UserAuthenticationModel?> TryLogin(UserModel User, string PlainTextPassword)
        {

            var UserAuth = await this.GetUserAuthentication(User).ConfigureAwait(false);

            return this.MatchingPassword(PlainTextPassword, UserAuth) ? UserAuth : null;

        }

        public async Task<bool> AddRefreshToken(UserModel User, RefreshToken RefreshToken)
        {

            // Because google is smart, it sees through our polymorphism and sees that the RefreshTokenDatabaseEntry
            // is actually RefreshToken. Therefore, we just shallow copy the data over to a real instance
            // of RefreshTokenDatabaseEntry lol

            var DbEntry = new RefreshTokenDatabaseEntry
            {
                Secret = RefreshToken.Secret,
                TokenId = RefreshToken.TokenId,
                ValidUntil = RefreshToken.ValidUntil
            };

            return (await Database.Collection("UsersAuthentication")
                                  .Document(User.UserId)
                                  .Collection("RefreshTokens")
                                  .Document(RefreshToken.TokenId)
                                  .SetDocumentAsync(DbEntry)
                                  .ConfigureAwait(false)) is not null;

        }

        #endregion

    }

}
