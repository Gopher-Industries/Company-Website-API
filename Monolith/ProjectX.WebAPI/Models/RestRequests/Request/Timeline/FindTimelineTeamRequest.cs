namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record FindTimelineTeamRequest
    {
        /// <summary>
        /// Optional: Find a student by their student Id
        /// </summary>
        /// <example>c3f4ce0d-393d-464a-8987-6d273146a76e</example>
        public string? TeamId { get; init; }

        /// <summary>
        /// Optional: Find a student by their name
        /// </summary>
        /// <example>Alexander Hamilton</example>
        public string? TeamName { get; init; }
    }
}
