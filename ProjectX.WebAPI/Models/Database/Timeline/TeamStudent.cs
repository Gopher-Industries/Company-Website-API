using Google.Cloud.Firestore;
using ProjectX.WebAPI.Models.RestRequests.Response;

namespace ProjectX.WebAPI.Models.Database.Timeline
{

    /// <summary>
    /// A student within Gopher Industries
    /// </summary>
    [FirestoreData]
    public class TeamStudent
    {

        [FirestoreDocumentId]
        public string StudentId { get; init; }

        /// <summary>
        /// The full name of the student
        /// </summary>
        [FirestoreProperty]
        public string FullName { get; init; }

        /// <summary>
        /// A link to the students linked in profile
        /// </summary>
        [FirestoreProperty]
        public string LinkedInProfile { get; init; }

        /// <summary>
        /// A picture of the student, base64 encoded
        /// </summary>
        [FirestoreProperty]
        public string DisplayPicture { get; init; }

        [FirestoreProperty]
        public string Role { get; init; }

        [FirestoreProperty]
        public string AreaOfSpecialization { get; init; }

        [FirestoreProperty]
        public string RemarkableAchievements { get; init; }

        [FirestoreProperty]
        public string TeamId { get; init; }

    }
}
