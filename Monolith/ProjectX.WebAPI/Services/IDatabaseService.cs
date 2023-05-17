using Google.Api.Gax.Grpc;
using Google.Api.Gax.Grpc.Gcp;
using Google.Api.Gax.Grpc.Rest;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Core;
using ProjectX.WebAPI.Models.Config;
using ProjectX.WebAPI.Models.Database;
using System.Diagnostics;

namespace ProjectX.WebAPI.Services
{
    public interface IDatabaseService
    {

        public Task Initialize(IConfiguration Config);

        public IDatabaseCollectionReference Collection(string CollectionPath);

        public Task<IReadOnlyList<T>> GetAllDocuments<T>(string CollectionPath) where T : class;

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

        private CancellationToken? DatabaseConnecting;
        private FirestoreDb DatabaseReference;
        private readonly ILogger<FirestoreDatabase> Logger;

        private FirestoreDb Database { 
            get
            {
                DatabaseConnecting?.WaitHandle.WaitOne();
                return DatabaseReference;
            } 
        }

        public FirestoreDatabase(IConfiguration Config, ILogger<FirestoreDatabase> Logger)
        {

            this.Logger = Logger;

        }

        /// <summary>
        /// Create the initial connection to the database.
        /// </summary>
        /// <returns>A constructed <see cref="FirestoreDb"/> class to access firestore through</returns>
        public async Task Initialize(IConfiguration Config)
        {

            //
            // To change any of the authentication / login in database setup,
            // please refer to this to change the project that this connects to.
            // https://cloud.google.com/docs/authentication/production
            //

            var TokenSource = new CancellationTokenSource();
            this.DatabaseConnecting = TokenSource.Token;

            //var DebugConnectionTimer = Stopwatch.StartNew();

            //var ff = new FirestoreClientBuilder()
            //{
            //    JsonCredentials = Config.GetJson("ApiKeys:FirestoreAccess")
            //}.Build();

            //this.Logger.LogInformation($"Taken {DebugConnectionTimer.ElapsedMilliseconds}ms to connect to firestore client");

            //ff.GetDocument(new GetDocumentRequest
            //{
            //    Name = "projects/prototypeprojectx/databases/(default)/documents/Users/2acbf664-3915-43ae-b988-631a9bc16580"
            //});

            //this.Logger.LogInformation($"Taken {DebugConnectionTimer.ElapsedMilliseconds}ms to connect to firestore client and read doc");

            var ConnectionTimer = Stopwatch.StartNew();

            // Connect to the firestore database
            DatabaseReference = await new FirestoreDbBuilder()
            {
                //ProjectId = Config["ApiKeys:FirestoreAccess:project_id"],
                //JsonCredentials = Config.GetJson("ApiKeys:FirestoreAccess"),
                ProjectId = "sit-22t3-gopher-websit-a242043",
                JsonCredentials = File.ReadAllText("projectx-credentials.json"),
                //Endpoint = "https://firestore.googleapis.com",
                GrpcChannelOptions = GrpcChannelOptions.Empty
                    .WithKeepAliveTime(TimeSpan.FromMinutes(1))
                    .WithKeepAliveTimeout(TimeSpan.FromMinutes(1))
                    .WithEnableServiceConfigResolution(false)
                    .WithMaxReceiveMessageSize(int.MaxValue),
                GrpcAdapter = GrpcNetClientAdapter.Default

            }.BuildAsync().ConfigureAwait(false);

            //Console.WriteLine($"Channel: { fb.Client.GrpcClient. }");

            this.Logger.LogInformation($"Taken {ConnectionTimer.ElapsedMilliseconds}ms to connect to firestore database");

            TokenSource.Cancel();

        }

        public async Task<IReadOnlyList<T>> GetAllDocuments<T>(string CollectionPath) where T : class
        {
            var querySnapshot = await this.Database.Collection(CollectionPath)
                                                 .GetSnapshotAsync()
                                                 .ConfigureAwait(false);

            return querySnapshot.Documents.Select(documentSnapshot => documentSnapshot.ConvertTo<T>()).ToList();
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
