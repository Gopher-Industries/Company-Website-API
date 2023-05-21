namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record UpdateTimelineStudentRequest
    {
        /// <summary>
        /// The ID of the student
        /// </summary>
        /// <example>is2lso20sna3msgJD8MD</example>
        public string StudentTimelineId { get; init; }

        /// <summary>
        /// The id of the student
        /// </summary>
        /// <example>232313213</example>
        public string? StudentId { get; init; }

        /// <summary>
        /// The full name of the student
        /// </summary>
        /// <example>John McFluffy</example>
        public string? FullName { get; init; }

        /// <summary>
        /// Title of the student's achievement
        /// </summary>
        /// <example>Successful elderly fall detection</example>
        public string? Title { get; init; }

        /// <summary>
        /// A short description (max 250 words) of the student's achievement
        /// </summary>
        public string? Description { get; init; }
    }
}
