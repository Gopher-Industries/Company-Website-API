using Database.Models;
using Google.Api.Gax.Grpc;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.Json;

namespace Database.Services
{
    /// <summary>
    /// Provides access to the firestore database.
    /// </summary>
    public class FirestoreDatabase : IDatabaseService
    {

        private CancellationToken? DatabaseConnecting;
        private FirestoreDb DatabaseReference;
        private readonly ILogger<FirestoreDatabase> Logger;
        private readonly string CollectionPathPrefix;

        private FirestoreDb Database
        {
            get
            {
                DatabaseConnecting?.WaitHandle.WaitOne();
                return DatabaseReference;
            }
        }

        public FirestoreDatabase(IOptions<FirestoreConfiguration> Configuration, ILogger<FirestoreDatabase> Logger)
        {

            this.Logger = Logger;

            this.CollectionPathPrefix = $"Environments/{GetEnvironmentType()}/";

            var ServiceAccount = JObject.Parse(Configuration.Value.FirestoreServiceAccountJsonAccessKey);

            //
            // Validate that Configuration contains all necissary information
            if (ServiceAccount is null)
                throw new ArgumentException("Missing section \"Credentials\" in configuration setup parsed to FirestoreDatabase service.");

            if (Configuration.Value.FirestoreServiceAccountJsonAccessKey is null)
                throw new ArgumentException("Missing information within parsed database configuration. Missing property \"project_id\" in credentials json. Isn't that a little necessary?");

            _ = Initialize(ServiceAccount["project_id"].ToString(), Configuration.Value.FirestoreServiceAccountJsonAccessKey);

        }

        /// <summary>
        /// Create the initial connection to the firestore database.
        /// </summary>
        /// <param name="ProjectId">The Project Id of the google cloud project the firestore database is hosted in.</param>
        /// <param name="AccessCredentialsJson">The json credentials for a service account within the google cloud project.</param>
        /// <returns></returns>
        private async Task Initialize(string ProjectId, string AccessCredentialsJson)
        {

            //
            // To change any of the authentication / login in database setup,
            // please refer to this to change the project that this connects to.
            // https://cloud.google.com/docs/authentication/production
            //

            var TokenSource = new CancellationTokenSource();
            DatabaseConnecting = TokenSource.Token;

            var ConnectionTimer = Stopwatch.StartNew();

            // Connect to the firestore database
            DatabaseReference = await new FirestoreDbBuilder()
            {
                ProjectId = ProjectId,
                JsonCredentials = AccessCredentialsJson,
                //Endpoint = "https://firestore.googleapis.com",
                GrpcChannelOptions = GrpcChannelOptions.Empty
                    .WithKeepAliveTime(TimeSpan.FromMinutes(1))
                    .WithKeepAliveTimeout(TimeSpan.FromMinutes(1))
                    .WithEnableServiceConfigResolution(false)
                    .WithMaxReceiveMessageSize(int.MaxValue),
                GrpcAdapter = GrpcNetClientAdapter.Default

            }.BuildAsync().ConfigureAwait(false);

            Logger.LogInformation($"Taken {ConnectionTimer.ElapsedMilliseconds}ms to connect to firestore database");

            DatabaseConnecting = null;

            TokenSource.Cancel();

        }

        public async Task<T?> GetDocument<T>(string CollectionPath, string DocumentId) where T : class
        {

            // Append the environment document path to the collection path
            CollectionPath = CollectionPathPrefix + CollectionPath;

            //
            // Collection Path Example:
            // Timeline/Collections/Students
            // Users
            // UsersAuthentication

            var CollectionReference = Database.Collection(CollectionPath);

            return (await CollectionReference.Document(DocumentId)
                                             .GetSnapshotAsync()
                                             .ConfigureAwait(false))
                                             .ConvertTo<T?>();

        }

        public IDatabaseCollectionReference Collection(string CollectionPath)
        {

            // Append the environment document path to the collection path
            CollectionPath = CollectionPathPrefix + CollectionPath;

            return new FirestoreDatabaseQueryBuilder(Database.Collection(CollectionPath));

        }

        public async Task<IReadOnlyList<T>> GetDocuments<T>(string CollectionPath, IEnumerable<string> DocumentIds) where T : class
        {

            // Append the environment document path to the collection path
            CollectionPath = CollectionPathPrefix + CollectionPath;

            return await Collection(CollectionPath)
                       .WhereIn(FieldPath.DocumentId.ToString(), DocumentIds)
                       .GetAsync<T>();

        }

        public async Task<bool> SetDocument<T>(string CollectionPath, string DocumentId, T Value) where T : class
        {

            // Append the environment document path to the collection path
            CollectionPath = CollectionPathPrefix + CollectionPath;

            var CollectionReference = Database.Collection(CollectionPath);

            return await CollectionReference.Document(DocumentId)
                                             .SetAsync(Value)
                                             .ConfigureAwait(false)
                                             is not null;

        }

        public async Task<bool> UpdateDocument(string CollectionPath, string DocumentId, params (string, object)[] Updates)
        {

            // Append the environment document path to the collection path
            CollectionPath = CollectionPathPrefix + CollectionPath;

            var CollectionReference = Database.Collection(CollectionPath);

            return await CollectionReference.Document(DocumentId)
                                             .UpdateAsync(Updates.ToDictionary(x => x.Item1, x => x.Item2))
                                             .ConfigureAwait(false)
                                             is not null;

        }

        public async Task<bool> DeleteDocument(string CollectionPath, string DocumentId)
        {

            // Append the environment document path to the collection path
            CollectionPath = CollectionPathPrefix + CollectionPath;

            return await Database.Document($"{CollectionPath}/{DocumentId}")
                                       .DeleteAsync()
                                       .ConfigureAwait(false)
                                       is not null;

        }

        public async Task<bool> DeleteDocuments(string CollectionPath, IEnumerable<string> DocumentIds)
        {

            // Append the environment document path to the collection path
            CollectionPath = CollectionPathPrefix + CollectionPath;

            var Batch = Database.StartBatch();
            foreach (var DocumentId in DocumentIds)
                Batch.Delete(Database.Document($"{CollectionPath}/{DocumentId}"));

            return await Batch.CommitAsync() is not null;

        }

        private static string GetEnvironmentType()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() switch
            {
                "development" => "Development",
                "staging" => "Staging",
                "production" => "Production",
                _ => throw new MissingFieldException("The environment variable 'ASPNETCORE_ENVIRONMENT' was not specified.")
            };
        }
    }
}
