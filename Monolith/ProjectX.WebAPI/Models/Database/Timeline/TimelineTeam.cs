using Google.Cloud.Firestore;

namespace ProjectX.WebAPI.Models.Database.Timeline
{
    [FirestoreData]
    public class TimelineTeam
    {
        
        [FirestoreDocumentId]
        public string TimelineTeamId { get; init; }

        /// <summary>
        /// The ID of the team
        /// </summary>
        [FirestoreDocumentId]
        public string TeamId { get; init; }

        /// <summary>
        /// The name of the team
        /// </summary>
        [FirestoreProperty]
        public string TeamName { get; set; }

        /// <summary>
        /// Title of achievement
        /// </summary>
        [FirestoreProperty]
        public string Title { get; set; }

        /// <summary>
        /// The description of the team
        /// </summary>
        [FirestoreProperty]
        public string Description { get; set; }

        /// <summary>
        /// Date at which the achievement of this team was added
        /// </summary>
        [FirestoreProperty]
        public DateTime Date { get; set; }

        /// <summary>
        /// The summary video for the team
        /// </summary>
        /// <example>T2 2022</example>
        [FirestoreProperty]
        public string VideoLink { get; init; }

        /// <summary>
        /// A link to the project prototype
        /// </summary>
        /// <example>T2 2022</example>
        [FirestoreProperty]
        public string PrototypeLink { get; init; }

    }
}
