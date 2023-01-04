using Google.Cloud.Firestore;

namespace Authentication.Models.Users
{

    /// <summary>
    /// A model for how we're going to store authentication data about users
    /// </summary>
    [FirestoreData]
    public class UserAuthenticationModel
    {

        [FirestoreDocumentId]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string Username { get; set; }

        [FirestoreProperty]
        public string Version { get; set; } = "0.1.0";

        [FirestoreProperty]
        public string HashedPassword { get; set; }

        [FirestoreProperty]
        public string Salt { get; set; }

        [FirestoreProperty]
        public string Pepper { get; set; }

        [FirestoreProperty]
        public string Role { get; set; }

    }
}
