namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record GetTimelineRequest
    {
        /// <summary>
        /// Required: type of timeline, specify either Student or Company
        /// </summary>
        /// <example>Student</example>
        public TimelineType timelineType { get; init; }
    }

    public enum TimelineType
    {
        Student,
        Company
    }
}
