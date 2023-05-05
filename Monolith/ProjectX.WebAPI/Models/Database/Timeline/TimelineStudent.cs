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

        public string TimelineStudentId { get; init; }

        [FirestoreDocumentId]
        /// <summary>
        /// The ID of the student
        /// </summary>
        public string StudentId { get; init; }

        /// <summary>
        /// The full name of the student
        /// </summary>
        [FirestoreProperty]
        public string FullName { get; set; }

        /// <summary>
        /// Title of achievement
        /// </summary>
        [FirestoreProperty]
        public string Title { get; set; }

        /// <summary>
        /// Description of achievement
        /// </summary>
        [FirestoreProperty]
        public string Description { get; set; }

        /// <summary>
        /// Date at which the achievement of this student was added
        /// </summary>
        [FirestoreProperty]
        public DateTime Date { get; set; }
    }
}
