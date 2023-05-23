using Google.Cloud.Firestore;

namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record CreateTimelineTeamRequest
    {
        /// <summary>
        /// The name of the team
        /// </summary>
        public string TeamName { get; init; }

        /// <summary>
        /// Required: Title of the team's achievement
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// Required: A short description (max 250 words) of the student's achievement
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// The summary video for the team
        /// </summary>
        /// <example>https://www.youtube.com/yourteamsummaryvideo</example>
        public string? VideoLink { get; init; }
    }
}
