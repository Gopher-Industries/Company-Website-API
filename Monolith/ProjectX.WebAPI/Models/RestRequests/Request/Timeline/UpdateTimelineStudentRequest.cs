namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record UpdateTimelineStudentRequest
    {
        /// <summary>
        /// The ID of the team
        /// </summary>
        public string StudentTimelineId { get; init; }

        /// <summary>
        /// The id of the team
        /// </summary>
        /// 
        public string? StudentId { get; init; }


        /// <summary>
        /// The full name of the team
        /// </summary>
        /// <example>John McFluffy</example>
        public string? FullName { get; init; }

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
