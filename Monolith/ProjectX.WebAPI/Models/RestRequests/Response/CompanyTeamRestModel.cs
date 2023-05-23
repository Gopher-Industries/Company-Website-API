using Google.Cloud.Firestore;
using ProjectX.WebAPI.Models.Database.Timeline;
using System.ComponentModel;

namespace ProjectX.WebAPI.Models.RestRequests.Response
{

    [DisplayName("Company Team")]
    [FirestoreData]
    public class CompanyTeamRestModel : TimelineTeam
    {

        /// <summary>
        /// The students involved with the team
        /// </summary>
        public List<TimelineStudent> Students { get; set; }

    }
}
