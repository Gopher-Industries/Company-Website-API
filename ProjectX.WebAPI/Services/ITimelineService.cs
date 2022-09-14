using Google.Cloud.Firestore;
using ProjectX.WebAPI.Models.Database.Timeline;
using ProjectX.WebAPI.Models.RestRequests.Request;
using ProjectX.WebAPI.Models.RestRequests.Response;

namespace ProjectX.WebAPI.Services
{
    public interface ITimelineService
    {

        public Task<IReadOnlyList<CompanyTeamRestModel>> GetTimeline(TimelineRequest Request);

    }

    public class TimelineService : ITimelineService
    {

        private readonly IDatabaseService Database;

        public TimelineService(IDatabaseService database)
        {
            Database = database;
        }

        public async Task<IReadOnlyList<CompanyTeamRestModel>> GetTimeline(TimelineRequest Request)
        {

            var FilteredTeamNames = Request.TeamName?
                                           .Split(',')
                                           .Select(x => x.Trim());

            var FilteredStudentIds = Request.StudentId?
                                            .Split(',')
                                            .Select(x => x.Trim())
                                            .ToArray();

            IReadOnlyList<TeamStudent>? StudentQuery = null;

            if (Request.StudentId is not null)
            {

                //StudentQuery = (await this.Database.Collection("Timeline")
                //                                   .Document("Collections")
                //                                   .Collection("Students")
                //                                   .WhereIn(FieldPath.DocumentId, FilteredStudentIds)
                //                                   .GetSnapshotAsync().ConfigureAwait(false))
                //                                   .Select(x => x.ConvertTo<TeamStudent>()).ToList();

                StudentQuery = await this.Database.Collection("Timeline")
                                                  .Document("Collections")
                                                  .Collection("Students")
                                                  .WhereIn(FieldPath.DocumentId.ToString(), FilteredStudentIds)
                                                  .GetAsync<TeamStudent>();

                // Filter the team names by the teams that the students belong to
                if (FilteredTeamNames is not null)
                {
                    FilteredTeamNames = StudentQuery.SelectMany(x => x.TeamId.Split(',')
                                                                             .Select(id => id.Trim()))
                                                    .Distinct()
                                                    .Where(x => FilteredTeamNames is null || FilteredTeamNames.Contains(x)).ToList();

                    if (FilteredTeamNames.Any() is false)
                        return new List<CompanyTeamRestModel>();

                }

            }

            //var TeamQuery = this.Database.Collection("Timeline")
            //                             .Document("Collections")
            //                             .Collection("Teams")
            //                             .WhereEqualTo(nameof(CompanyTeam.Trimester), Request.Trimester)
            //                             as Query;
            var TeamQuery = this.Database.Collection("Timeline")
                                         .Document("Collections")
                                         .Collection("Teams")
                                         .WhereEqual(nameof(CompanyTeam.Trimester), Request.Trimester);

            // No filter on Team Name. Return all teams.
            if (FilteredTeamNames is not null)
            {
                // Select only teams with specific team names
                TeamQuery = TeamQuery.WhereIn(nameof(CompanyTeam.TeamName), FilteredTeamNames);
            }

            //Teams = (await TeamQuery.GetSnapshotAsync().ConfigureAwait(false))
            //                    .Select(x => x.ConvertTo<CompanyTeamRestModel>())
            //                    .ToList();
            var Teams = await TeamQuery.GetAsync<CompanyTeamRestModel>();

            //StudentQuery ??= (await this.Database.Collection("Timeline")
            //                                     .Document("Collections")
            //                                     .Collection("Students")
            //                                     .WhereIn(nameof(TeamStudent.TeamId), Teams.Select(x => x.TeamId))
            //                                     .GetSnapshotAsync().ConfigureAwait(false))
            //                                     .Select(x => x.ConvertTo<TeamStudent>()).ToList();

            if (Teams.Any() is false)
                return Teams;

            StudentQuery ??= await this.Database.Collection("Timeline")
                                                .Document("Collections")
                                                .Collection("Students")
                                                .WhereIn(nameof(TeamStudent.TeamId), Teams.Select(x => x.TeamId))
                                                .GetAsync<TeamStudent>();

            foreach (var Team in Teams)
                Team.Students = StudentQuery.Where(x => x.TeamId == Team.TeamId).ToList();

            return Teams;

        }

    }

}
