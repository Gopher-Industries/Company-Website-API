namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record FindTimelineStudentRequest
    {

        /// <summary>
        /// Optional: Find a student by their student Id
        /// </summary>
        /// <example>220211323</example>
        public string? StudentId { get; set; }

        /// <summary>
        /// Optional: Find a student by their name
        /// </summary>
        /// <example>Alexander Hamilton</example>
        public string? StudentName { get; set; }

    }
}
