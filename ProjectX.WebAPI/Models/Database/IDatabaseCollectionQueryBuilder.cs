using Google.Cloud.Dialogflow.V2;
using Google.Cloud.Firestore;
using ProjectX.WebAPI.Services;

namespace ProjectX.WebAPI.Models.Database
{
    public interface IDatabaseCollectionQueryBuilder
    {
        IDatabaseCollectionQueryBuilder WhereArrayContains(string FieldName, object Value);
        IDatabaseCollectionQueryBuilder WhereArrayContainsAny(string FieldName, IEnumerable<object> Value);
        IDatabaseCollectionQueryBuilder WhereEqual(string FieldName, object Value);
        IDatabaseCollectionQueryBuilder WhereGreaterThan(string FieldName, object Value);
        IDatabaseCollectionQueryBuilder WhereGreaterThanOrEqual(string FieldName, object Value);
        IDatabaseCollectionQueryBuilder WhereIn(string FieldName, IEnumerable<object> Value);
        IDatabaseCollectionQueryBuilder WhereLessThan(string FieldName, object Value);
        IDatabaseCollectionQueryBuilder WhereLessThanOrEqual(string FieldName, object Value);
        IDatabaseCollectionQueryBuilder WhereNotEqual(string FieldName, object Value);
        IDatabaseCollectionQueryBuilder WhereNotIn(string FieldName, IEnumerable<object> Value);
        IDatabaseCollectionQueryBuilder Limit(int Count);
        Task<IReadOnlyList<T>> GetAsync<T>() where T : class;
        Task<IReadOnlyList<T>> DeleteAsync<T>() where T : class;
    }

    public interface IDatabaseCollectionReference : IDatabaseCollectionQueryBuilder
    {
        IDatabaseDocumentReference Document(string DocumentId);
    }

    public interface IDatabaseDocumentReference
    {
        IDatabaseCollectionReference Collection(string CollectionName);
        Task<T> CreateDocumentAsync<T>(T Value) where T : class;
        Task<T> GetDocumentAsync<T>() where T : class;
        Task<T> DeleteDocumentAsync<T>() where T : class;
        Task<T> SetDocumentAsync<T>(T Value) where T : class;
    }

    public class FirestoreDatabaseQueryBuilder : IDatabaseCollectionQueryBuilder, IDatabaseCollectionReference, IDatabaseDocumentReference
    {

        private Query? _query;

        // Here to help build the initial reference to a collection.
        private string? CurrentDocumentReference = null;
        private CollectionReference CurrentCollectionReference;

        public FirestoreDatabaseQueryBuilder(CollectionReference Collection)
        {
            CurrentCollectionReference = Collection;
        }

        public IDatabaseDocumentReference Document(string DocumentId)
        {
            CurrentDocumentReference = DocumentId;
            return this;
        }

        public IDatabaseCollectionReference Collection(string CollectionName)
        {
            CurrentCollectionReference = CurrentCollectionReference.Document(CurrentDocumentReference)
                                                                   .Collection(CollectionName);
            CurrentDocumentReference = null;
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereLessThan(string FieldName, object Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereLessThan(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereLessThanOrEqual(string FieldName, object Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereLessThanOrEqualTo(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereGreaterThan(string FieldName, object Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereGreaterThan(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereGreaterThanOrEqual(string FieldName, object Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereGreaterThanOrEqualTo(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereEqual(string FieldName, object Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereEqualTo(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereNotEqual(string FieldName, object Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereNotEqualTo(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereArrayContains(string FieldName, object Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereArrayContains(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereIn(string FieldName, IEnumerable<object> Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereIn(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereArrayContainsAny(string FieldName, IEnumerable<object> Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereArrayContainsAny(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder WhereNotIn(string FieldName, IEnumerable<object> Value)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.WhereNotIn(FieldName, Value);
            return this;
        }

        public IDatabaseCollectionQueryBuilder Limit(int Count)
        {
            _query ??= CurrentCollectionReference;
            _query = _query.Limit(Count);
            return this;
        }

        public async Task<T> CreateDocumentAsync<T>(T Value) where T : class
        {

            if (CurrentDocumentReference is null)
                throw new InvalidOperationException("No document referenced! How did we even get here??");

            await CurrentCollectionReference.Document(CurrentDocumentReference)
                                            .CreateAsync(Value)
                                            .ConfigureAwait(false);

            return Value;

        }

        public async Task<T> GetDocumentAsync<T>() where T : class
        {

            if (CurrentDocumentReference is null)
                throw new InvalidOperationException("No document referenced! How did we even get here??");

            return (await CurrentCollectionReference.Document(CurrentDocumentReference).GetSnapshotAsync().ConfigureAwait(false)).ConvertTo<T>();


        }

        public async Task<IReadOnlyList<T>> GetAsync<T>() where T : class
        {
            //_query ??= CurrentCollectionReference;
            return (await _query.GetSnapshotAsync().ConfigureAwait(false))
                                .Select(x => x.ConvertTo<T>()).ToList();
        }

        public async Task<T> SetDocumentAsync<T>(T Value) where T : class
        {

            _query ??= CurrentCollectionReference;

            await CurrentCollectionReference.Document(CurrentDocumentReference)
                                            .SetAsync(Value)
                                            .ConfigureAwait(false);
            
            return Value;

        }

        public async Task<T> DeleteDocumentAsync<T>() where T : class
        {

            _query ??= CurrentCollectionReference;

            var Doc = await CurrentCollectionReference.Document(CurrentDocumentReference)
                                                      .GetSnapshotAsync()
                                                      .ConfigureAwait(false);
            var DocAsType = Doc.ConvertTo<T>();

            await Doc.Reference.DeleteAsync();

            return DocAsType;

        }

        public async Task<IReadOnlyList<T>> DeleteAsync<T>() where T : class
        {

            _query ??= CurrentCollectionReference;

            var AllDocs = await _query.GetSnapshotAsync().ConfigureAwait(false);
            var DocsAsType = AllDocs.Select(x => x.ConvertTo<T>()).ToList();

            var Batch = this._query.Database.StartBatch();
            foreach (var Document in AllDocs)
                Batch.Delete(Document.Reference);

            await Batch.CommitAsync();

            return DocsAsType;

        }

    }

}