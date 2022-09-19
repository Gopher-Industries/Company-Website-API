namespace ProjectX.WebAPI.Models.RestRequests.Request.Timeline
{
    public record FindTimelineTeamRequest
    {

        public string Trimester { get; init; }

        public string TeamName { get; init; }

    }
}
