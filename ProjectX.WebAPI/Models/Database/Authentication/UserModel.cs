using Google.Cloud.Firestore;

namespace ProjectX.WebAPI.Models.Database.Authentication
{

    /// <summary>
    /// Model of the user document in the Firestore database.
    /// </summary>
    [FirestoreData]
    public class UserModel
    {

        private UserModel()
        {

        }

        public UserModel(string UserId,
                         string Username,
                         string Email,
                         string Organisation,
                         DateTime DateOfBirth)
        {
            this.UserId = UserId;
            this.Username = Username;
            this.Email = Email;
            this.Organisation = Organisation;
            EmailVerified = false;
            Created = DateTime.UtcNow;
            this.DateOfBirth = DateOfBirth;
            ExistsInDatabase = false;
        }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string Username { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string Organisation { get; }

        [FirestoreProperty]
        public bool EmailVerified { get; set; }

        [FirestoreProperty]
        public DateTime Created { get; set; }

        [FirestoreProperty]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Calculate the users age from their date of birth
        /// </summary>
        public TimeSpan Age => DateTime.Now - DateOfBirth;

        internal bool ExistsInDatabase { get; } = true;

    }
}
