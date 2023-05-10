namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record UpdateTimelineTeamRequest
    {
        /// <summary>
        /// The ID of the team
        /// </summary>
        public string TeamTimelineId { get; init; }

        /// <summary>
        /// The id of the team
        /// </summary>
        /// 
        public string? TeamId { get; init; }


        /// <summary>
        /// The full name of the team
        /// </summary>
        /// <example>John McFluffy</example>
        public string? TeamName { get; init; }

        /// <summary>
        /// Title of the student's achievement
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// A short description (max 250 words) of the student's achievement
        /// </summary>
        public string? Description { get; init; }

    }
}
