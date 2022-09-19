using Google.Cloud.Firestore;

namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record CreateTimelineTeamRequest
    {

        /// <summary>
        /// The name of the team
        /// </summary>
        /// <example>Team Avengers</example>
        public string TeamName { get; init; }

        /// <summary>
        /// The description of the team
        /// </summary>
        /// <example>Team Avengers</example>
        public string Description { get; init; }

        /// <summary>
        /// The logo of the team, base64 encoded
        /// </summary>
        public string Logo { get; init; }

        /// <summary>
        /// The trimester that the team work in
        /// </summary>
        /// <example>T2 2022</example>
        public string Trimester { get; init; }

        /// <summary>
        /// The summary video for the team
        /// </summary>
        /// <example>https://www.youtube.com/yourteamsummaryvideo</example>
        public string VideoLink { get; init; }

        /// <summary>
        /// A link to the project prototype
        /// </summary>
        /// <example>https://api.gopherindustries.net/swagger/index.html</example>
        public string PrototypeLink { get; init; }

        /// <summary>
        /// The mentors that helped the team
        /// </summary>
        /// <example>Jenny Hummings, Olivia Ham</example>
        public string[] Mentors { get; init; }

    }
}
