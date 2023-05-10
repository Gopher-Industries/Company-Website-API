namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record UpdateTimelineStudentRequest
    {

        /// <summary>
        /// The full name of the student
        /// </summary>
        /// <example>John McFluffy</example>
        public string? FullName { get; init; }

        /// <summary>
        /// A link to the students linked in profile
        /// </summary>
        /// <example>https://au.linkedin.com/in/john-mcfluffy</example>
        public string? LinkedInProfile { get; init; }

        /// <summary>
        /// A picture of the student, base64 encoded
        /// </summary>
        public string? ProfilePicture { get; init; }

        /// <summary>
        /// The role of the user
        /// </summary>
        /// <example>Scrum Master, Product Owner, or Developer</example>
        public string ?Role { get; init; }

        /// <summary>
        /// Area where the student specialized
        /// </summary>
        /// <example>Frontend Development, UI/UX Design, Artificial Intelligence, etc.</example>
        public string? AreaOfSpecialization { get; init; }

        /// <summary>
        /// Any remarkable achievements of the student
        /// </summary>
        public string? RemarkableAchievements { get; init; }

        /// <summary>
        /// The team name that the student worked on
        /// </summary>
        public string? TeamName { get; init; }

        /// <summary>
        /// The trimester of the given team. Required when TeamName is specified.
        /// </summary>
        public string? TeamTrimester { get; init; }

        /// <summary>
        /// The teams that the student belonged to
        /// </summary>
        public TimelineTeamReference[]? Teams { get; init; }

    }
}
