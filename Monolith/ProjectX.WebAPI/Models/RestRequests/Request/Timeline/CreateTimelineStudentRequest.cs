using Google.Cloud.Firestore;

namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record CreateTimelineStudentRequest
    {

        /// <summary>
        /// Required: The ID of the student
        /// </summary>
        /// <example>229823231</example>
        public string StudentId { get; init; }

        /// <summary>
        /// Required: The full name of the student
        /// </summary>
        /// <example>John McFluffy</example>
        public string FullName { get; init; }

        /// <summary>
        /// Required: Title of the student's achievement
        /// </summary>
        /// <example>Frontend Development, UI/UX Design, Artificial Intelligence, etc.</example>
        public string Title { get; init; }

        /// <summary>
        /// Required: A short description (max 250 words) of the student's achievement
        /// </summary>
        public string Description { get; init; }

    }

}
