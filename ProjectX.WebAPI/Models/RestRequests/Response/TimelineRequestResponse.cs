using ProjectX.WebAPI.Models.Database.Timeline;

namespace ProjectX.WebAPI.Models.RestRequests.Response
{
    public class TimelineRequestResponse
    {

        /// <summary>
        /// The teams within the timeline
        /// </summary>
        public List<CompanyTeamRestModel> Teams { get; init; }

    }

}
