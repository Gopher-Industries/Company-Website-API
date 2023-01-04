namespace Database.Models
{
    public sealed record FirestoreConfiguration
    {

        /// <summary>
        /// The JSON access key issued via Google Cloud Platform for a service account 
        /// that has permissions to read / write to the firestore database
        /// </summary>
        public string FirestoreServiceAccountJsonAccessKey { get; init; }

    }
}
