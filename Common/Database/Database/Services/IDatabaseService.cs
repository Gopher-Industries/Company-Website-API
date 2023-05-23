using Database.Models;
using Microsoft.Extensions.Configuration;

namespace Database.Services
{

    /// <summary>
    /// A service that enables reading and writing access to a NoSQL Database 
    /// </summary>
    public interface IDatabaseService
    {

        /// <summary>
        /// Retrieve a reference to a collection (folder) of documents.
        /// </summary>
        /// <param name="CollectionPath"></param>
        /// <returns></returns>
        public IDatabaseCollectionReference Collection(string CollectionPath);

        /// <summary>
        /// Retrieves a reference to a document in a collection (folder) of documents, 
        /// based on the <paramref name="CollectionPath"/> (folder path) and the unique <paramref name="DocumentId"/>.
        /// </summary>
        /// <typeparam name="T">The database model of the information read from the NoSQL document</typeparam>
        /// <param name="CollectionPath">The folder path that the documents are contained inside of. The parent folder.</param>
        /// <param name="DocumentId">The unique Document Id of the document wanting to be retrieved.</param>
        /// <returns></returns>
        public Task<T?> GetDocument<T>(string CollectionPath, string DocumentId) where T : class;

        /// <summary>
        /// Retrieves references to several documents inside the collection contained in the <paramref name="CollectionPath"/>,
        /// and the specific documents you want given by the <paramref name="DocumentIds"/>
        /// </summary>
        /// <typeparam name="T">The database model of the information read from the NoSQL document</typeparam>
        /// <param name="CollectionPath">The folder path that the documents are contained inside of. The parent folder.</param>
        /// <param name="DocumentIds">The unique Document Ids of the documents wanting to be read. The output will be in the same order as this.</param>
        /// <returns>A list of NoSQL documents retrieved from the database</returns>
        public Task<IReadOnlyList<T>> GetDocuments<T>(string CollectionPath, IEnumerable<string> DocumentIds) where T : class;

        /// <summary>
        /// Overwrite the given document completely and replaces its information with the information inside of <paramref name="Value"/>. 
        /// <para/>
        /// The <paramref name="CollectionPath"/> specifies the parent collection path, 
        /// and the specific document you want given by the <paramref name="DocumentId"/>.
        /// </summary>
        /// <typeparam name="T">The database model of the information read from the NoSQL document</typeparam>
        /// <param name="CollectionPath">The folder path that the documents are contained inside of. The parent folder.</param>
        /// <param name="DocumentId">The unique Document Id of the document wanting to be overwritten.</param>
        /// <param name="Value">The model to overwrite the existing document</param>
        /// <returns>An indication whether the action was successful</returns>
        public Task<bool> SetDocument<T>(string CollectionPath, string DocumentId, T Value) where T : class;

        /// <summary>
        /// Updates or adds information inside of <paramref name="Updates"/> that isn't null or empty to the NoSQL database. <br/>
        /// Use this to update specific values inside a document. 
        /// <para/>
        /// The <paramref name="CollectionPath"/> specifies the parent collection path, 
        /// and the specific document you want given by the <paramref name="DocumentId"/>.
        /// </summary>
        /// <param name="CollectionPath">The folder path that the documents are contained inside of. The parent folder.</param>
        /// <param name="DocumentId">The unique Document Id of the document wanting to be updated.</param>
        /// <returns>An indication whether the action was successful</returns>
        public Task<bool> UpdateDocument(string CollectionPath, string DocumentId, params (string, object)[] Updates);

        /// <summary>
        /// Completely deletes the document found inside <paramref name="CollectionPath"/> with the given <paramref name="DocumentId"/>.
        /// </summary>
        /// <typeparam name="T">The database model of the information read from the NoSQL document</typeparam>
        /// <param name="CollectionPath">The folder path that the documents are contained inside of. The parent folder.</param>
        /// <param name="DocumentId">The unique Document Id of the document wanting to be deleted.</param>
        /// <returns>An indication whether the action was successful</returns>
        public Task<bool> DeleteDocument(string CollectionPath, string DocumentId);

        /// <summary>
        /// Retrieves references to several documents inside the collection contained in the <paramref name="CollectionPath"/>,
        /// and the specific documents you want given by the <paramref name="DocumentIds"/>
        /// </summary>
        /// <typeparam name="T">The database model of the information read from the NoSQL document</typeparam>
        /// <param name="CollectionPath">The folder path that the documents are contained inside of. The parent folder.</param>
        /// <param name="DocumentIds">The unique Document Ids of the documents wanting to be deleted.</param>
        /// <returns>An indication whether the action was successful</returns>
        public Task<bool> DeleteDocuments(string CollectionPath, IEnumerable<string> DocumentIds);

    }
}
