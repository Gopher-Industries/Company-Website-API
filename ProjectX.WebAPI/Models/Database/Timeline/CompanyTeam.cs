using Google.Cloud.Firestore;

namespace ProjectX.WebAPI.Models.Database.Timeline
{
    [FirestoreData]
    public class CompanyTeam
    {

        /// <summary>
        /// The unique Id of the team within the company
        /// </summary>
        [FirestoreDocumentId]
        public string TeamId { get; init; }

        /// <summary>
        /// The name of the team
        /// </summary>
        /// <example>Team Avengers</example>
        [FirestoreProperty]
        public string TeamName { get; init; }

        /// <summary>
        /// The description of the team
        /// </summary>
        /// <example>Team Avengers</example>
        [FirestoreProperty]
        public string Description { get; init; }

        /// <summary>
        /// The logo of the team, base64 encoded
        /// </summary>
        [FirestoreProperty]
        public string Logo { get; init; }

        /// <summary>
        /// The trimester that the team work in
        /// </summary>
        /// <example>T2 2022</example>
        [FirestoreProperty]
        public string Trimester { get; init; }

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

        /// <summary>
        /// The trimester that the team work in
        /// </summary>
        /// <example>T2 2022</example>
        [FirestoreProperty]
        public string[] Mentors { get; init; }

    }
}
