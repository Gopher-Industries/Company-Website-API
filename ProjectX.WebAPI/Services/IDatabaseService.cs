using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using ProjectX.WebAPI.Models.Config;
using ProjectX.WebAPI.Models.Database;
using System.Diagnostics;

namespace ProjectX.WebAPI.Services
{
    public interface IDatabaseService
    {

        public IDatabaseCollectionReference Collection(string CollectionPath);

        public Task<T> GetDocument<T>(string CollectionPath, string DocumentId) where T : class;

        public Task<IReadOnlyList<T>> GetDocuments<T>(string CollectionPath, IEnumerable<string> DocumentIds) where T : class;

        public Task<bool> SetDocument<T>(string CollectionPath, string DocumentId, T Value) where T : class;

        public Task<bool> UpdateDocument<T>(string CollectionPath, string DocumentId, params (string, object)[] Updates) where T : class;

        public Task<bool> DeleteDocument<T>(string CollectionPath, string DocumentId) where T : class;

        public Task<bool> DeleteDocuments<T>(string CollectionPath, IEnumerable<string> DocumentIds) where T : class;

    }

    /// <summary>
    /// Provides access to the firestore database.
    /// </summary>
    public class FirestoreDatabase : IDatabaseService
    {

        private readonly FirestoreDb Database;
        private readonly ILogger<FirestoreDatabase> Logger;

        public FirestoreDatabase(IConfiguration Config, ILogger<FirestoreDatabase> Logger)
        {
            
            this.Logger = Logger;
            this.Database = this.Initialize(Config);

        }

        /// <summary>
        /// Create the initial connection to the database.
        /// </summary>
        /// <returns>A constructed <see cref="FirestoreDb"/> class to access firestore through</returns>
        private FirestoreDb Initialize(IConfiguration Config)
        {

            //
            // To change any of the authentication / login in database setup,
            // please refer to this to change the project that this connects to.
            // https://cloud.google.com/docs/authentication/production
            //

            var ConnectionTimer = Stopwatch.StartNew();

            // Retrieve the credentials from the secrets

            var FirestoreAccess = Config.GetJson("ApiKeys:FirestoreAccess");

            this.Logger.LogInformation($"Taken {ConnectionTimer.ElapsedMilliseconds}ms to connect to read firestore settings");

            ConnectionTimer.Restart();

            var FirestoreClient = new FirestoreClientBuilder
            {
                JsonCredentials = FirestoreAccess,
            }.Build();

            // Connect to the firestore database
            var fb = FirestoreDb.Create(projectId: "prototypeprojectx", client: FirestoreClient);

            this.Logger.LogInformation($"Taken {ConnectionTimer.ElapsedMilliseconds}ms to connect to firestore database");

            return fb;

        }

        public async Task<T> GetDocument<T>(string CollectionPath, string DocumentId) where T : class
        {

            //
            // Collection Path Example:
            // Timeline/Collections/Students
            // Users
            // UsersAuthentication

            var CollectionReference = this.Database.Collection(CollectionPath);

            return (await CollectionReference.Document(DocumentId)
                                             .GetSnapshotAsync()
                                             .ConfigureAwait(false))
                                             .ConvertTo<T>();

        }

        public IDatabaseCollectionReference Collection(string CollectionPath)
        {
            return new FirestoreDatabaseQueryBuilder(this.Database.Collection(CollectionPath));
        }

        public async Task<IReadOnlyList<T>> GetDocuments<T>(string CollectionPath, IEnumerable<string> DocumentIds) where T : class
        {
            return await this.Collection(CollectionPath)
                       .WhereIn(FieldPath.DocumentId.ToString(), DocumentIds)
                       .GetAsync<T>();
        }

        public async Task<bool> SetDocument<T>(string CollectionPath, string DocumentId, T Value) where T : class
        {

            var CollectionReference = this.Database.Collection(CollectionPath);

            return (await CollectionReference.Document(DocumentId)
                                             .SetAsync(Value)
                                             .ConfigureAwait(false))
                                             is not null;
            
        }

        public async Task<bool> UpdateDocument<T>(string CollectionPath, string DocumentId, params (string, object)[] Updates) where T : class
        {

            var CollectionReference = this.Database.Collection(CollectionPath);

            return (await CollectionReference.Document(DocumentId)
                                             .UpdateAsync(Updates.ToDictionary(x => x.Item1, x => x.Item2))
                                             .ConfigureAwait(false))
                                             is not null;

        }

        public async Task<bool> DeleteDocument<T>(string CollectionPath, string DocumentId) where T : class
        {

            return (await this.Database.Document($"{CollectionPath}/{DocumentId}")
                                       .DeleteAsync()
                                       .ConfigureAwait(false)) 
                                       is not null;

        }

        public async Task<bool> DeleteDocuments<T>(string CollectionPath, IEnumerable<string> DocumentIds) where T : class
        {

            var Batch = this.Database.StartBatch();
            foreach (var DocumentId in DocumentIds)
                Batch.Delete(this.Database.Document($"{CollectionPath}/{DocumentId}"));

            return (await Batch.CommitAsync()) is not null;

        }
    }
}
