using Google.Cloud.Firestore;
using ProjectX.WebAPI.Models.RestRequests.Response;

namespace ProjectX.WebAPI.Models.Database.Timeline
{

    /// <summary>
    /// A student within Gopher Industries
    /// </summary>
    [FirestoreData]
    public class TimelineStudent
    {

        [FirestoreDocumentId]
        public string StudentId { get; init; }

        /// <summary>
        /// The full name of the student
        /// </summary>
        [FirestoreProperty]
        public string FullName { get; set; }

        /// <summary>
        /// A link to the students linked in profile
        /// </summary>
        [FirestoreProperty]
        public string LinkedInProfile { get; set; }

        /// <summary>
        /// A link to the users profile picture
        /// </summary>
        [FirestoreProperty]
        public string ProfilePicture { get; set; }

        /// <summary>
        /// The role of the user
        /// </summary>
        /// <example>Scrum Master, Product Owner, or Developer</example>
        [FirestoreProperty]
        public string Role { get; set; }

        /// <summary>
        /// Area where the student specialized
        /// </summary>
        /// <example>Frontend Development, UI/UX Design, Artificial Intelligence, etc.</example>
        [FirestoreProperty]
        public string AreaOfSpecialization { get; set; }

        /// <summary>
        /// Any remarkable achievements of the student
        /// </summary>
        [FirestoreProperty]
        public string RemarkableAchievements { get; set; }

        /// <summary>
        /// The id of the team that the student belongs to
        /// </summary>
        [FirestoreProperty]
        public string[] Teams { get; set; }

    }
}
