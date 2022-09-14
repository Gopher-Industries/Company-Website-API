namespace ProjectX.WebAPI.Models.RestRequests.Request
{
    public record TimelineRequest
    {

        /// <summary>
        /// Required: Filter by the trimester
        /// </summary>
        /// <example>T2 2022</example>
        public string Trimester { get; init; }

        /// <summary>
        /// Optional: Filter by the team name. Seperate multiple with a comma
        /// </summary>
        /// <example>Team Avengers, ELT, Team Guardians</example>
        public string? TeamName { get; init; }

        /// <summary>
        /// Optional: Filter by the student id
        /// </summary>
        public string? StudentId { get; init; }

    }
}
