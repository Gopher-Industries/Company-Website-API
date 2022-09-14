using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using ProjectX.WebAPI.Models.Config;
using ProjectX.WebAPI.Models.Database;

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

        public FirestoreDatabase(IConfiguration Config)
        {
            Database = InitializeDatabaseConnection(Config).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create the initial connection to the database.
        /// </summary>
        /// <returns>A constructed <see cref="FirestoreDb"/> class to access firestore through</returns>
        private async Task<FirestoreDb> InitializeDatabaseConnection(IConfiguration Config)
        {

            //
            // To change any of the authentication / login in database setup,
            // please refer to this to change the project that this connects to.
            // https://cloud.google.com/docs/authentication/production
            //

            // Retrieve the credentials from the secrets
            var FirestoreBuilder = new FirestoreClientBuilder
            {
                JsonCredentials = Config.GetJson("Credentials:FirestoreAccess")
            };

            // Connect to the firestore database
            return await FirestoreDb.CreateAsync(projectId: "prototypeprojectx", client: FirestoreBuilder.Build()).ConfigureAwait(false);

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

    //public enum DatabaseFilterOperation
    //{
    //    LessThan = 1,
    //    LessThanOrEqual = 2,
    //    GreaterThan = 3,
    //    GreaterThanOrEqual = 4,
    //    Equal = 5,
    //    NotEqual = 6,
    //    ArrayContains = 7,
    //    In = 8,
    //    ArrayContainsAny = 9,
    //    NotIn = 10,
    //}

    //public record DatabaseFilter
    //{

    //    /// <summary>
    //    /// The field name to perform the filter on
    //    /// </summary>
    //    public string FieldName { get; init; }

    //    public DatabaseFilterOperation Operation { get; init; }

    //    /// <summary>
    //    /// The value to compare against
    //    /// </summary>
    //    public object Value { get; init; }

    //    private DatabaseFilter()
    //    {

    //    }

    //    public static DatabaseFilter WhereLessThan(string FieldName, object Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.LessThan,
    //        Value = Value
    //    };

    //    public static DatabaseFilter WhereLessThanOrEqual(string FieldName, object Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.LessThanOrEqual,
    //        Value = Value
    //    };

    //    public static DatabaseFilter WhereGreaterThan(string FieldName, object Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.GreaterThan,
    //        Value = Value
    //    };

    //    public static DatabaseFilter WhereGreaterThanOrEqual(string FieldName, object Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.GreaterThanOrEqual,
    //        Value = Value
    //    };

    //    public static DatabaseFilter WhereEqual(string FieldName, object Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.Equal,
    //        Value = Value
    //    };

    //    public static DatabaseFilter WhereNotEqual(string FieldName, object Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.NotEqual,
    //        Value = Value
    //    };

    //    public static DatabaseFilter WhereArrayContains(string FieldName, object Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.ArrayContains,
    //        Value = Value
    //    };

    //    public static DatabaseFilter WhereIn(string FieldName, IEnumerable<object> Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.In,
    //        Value = Value
    //    };

    //    public static DatabaseFilter WhereArrayContainsAny(string FieldName, IEnumerable<object> Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.ArrayContainsAny,
    //        Value = Value
    //    };

    //    public static DatabaseFilter WhereNotIn(string FieldName, IEnumerable<object> Value) => new DatabaseFilter
    //    {
    //        FieldName = FieldName,
    //        Operation = DatabaseFilterOperation.NotIn,
    //        Value = Value
    //    };

    //}
}
