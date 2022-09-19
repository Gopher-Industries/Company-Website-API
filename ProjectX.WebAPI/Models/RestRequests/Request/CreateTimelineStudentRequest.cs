using Google.Cloud.Firestore;

namespace ProjectX.WebAPI.Models.RestRequests.Request
{
    public record CreateTimelineStudentRequest
    {

        /// <summary>
        /// Required: The full name of the student
        /// </summary>
        /// <example>John McFluffy</example>
        public string FullName { get; init; }

        /// <summary>
        /// Required: A link to the students linked in profile
        /// </summary>
        /// <example>https://au.linkedin.com/in/john-mcfluffy</example>
        public string LinkedInProfile { get; init; }

        /// <summary>
        /// Required: A picture of the student, base64 encoded
        /// </summary>
        public string DisplayPicture { get; init; }

        /// <summary>
        /// Required: The role of the user
        /// </summary>
        /// <example>Scrum Master, Product Owner, or Developer</example>
        public string Role { get; init; }

        /// <summary>
        /// Required: Area where the student specialized
        /// </summary>
        /// <example>Frontend Development, UI/UX Design, Artificial Intelligence, etc.</example>
        public string AreaOfSpecialization { get; init; }

        /// <summary>
        /// Required: Any remarkable achievements of the student
        /// </summary>
        public string RemarkableAchievements { get; init; }

        /// <summary>
        /// Optional: The team name that the student worked on
        /// </summary>
        public string TeamName { get; init; }

        /// <summary>
        /// Optional: The trimester of the given team. Required when TeamName is specified.
        /// </summary>
        public string TeamTrimester { get; init; }

    }
}
