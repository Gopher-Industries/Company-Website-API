using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ProjectX.WebAPI.Models.Database.Timeline;
using ProjectX.WebAPI.Models.RestRequests.Request.Timeline;
using ProjectX.WebAPI.Models.RestRequests.Response;

namespace ProjectX.WebAPI.Services
{
    public interface ITimelineService
    {

        public Task<IReadOnlyList<CompanyTeamRestModel>> GetTimeline(TimelineRequest Request);

        /// <summary>
        /// Creates a new student in the timeline
        /// </summary>
        /// <param name="Request"></param>
        /// <exception cref="ArgumentException"/>
        /// <returns></returns>
        public Task<TimelineStudent> CreateStudent(CreateTimelineStudentRequest Request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <exception cref="ArgumentException"/>
        /// <returns></returns>
        public Task<TimelineStudent?> GetStudent(FindStudentRequest Request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <exception cref="ArgumentException"/>
        /// <returns></returns>
        public Task<TimelineStudent?> DeleteStudent(string StudentId);

    }

    public class TimelineService : ITimelineService
    {

        private readonly IMemoryCache Cache;
        private readonly IDatabaseService Database;

        private static readonly MemoryCacheEntryOptions _timelinePersonCacheOptions = new MemoryCacheEntryOptions()
        {
            Size = 400,
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };

        public TimelineService(IDatabaseService database, IMemoryCache cache)
        {
            Database = database;
            Cache = cache;
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

            IReadOnlyList<TimelineStudent>? StudentQuery = null;

            if (Request.StudentId is not null)
            {

                StudentQuery = await this.Database.Collection("Timeline")
                                                  .Document("Collections")
                                                  .Collection("Students")
                                                  .WhereIn(FieldPath.DocumentId.ToString(), FilteredStudentIds)
                                                  .GetAsync<TimelineStudent>();

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

            var TeamQuery = this.Database.Collection("Timeline")
                                         .Document("Collections")
                                         .Collection("Teams")
                                         .WhereEqual(nameof(TimelineTeam.Trimester), Request.Trimester);

            // No filter on Team Name. Return all teams.
            if (FilteredTeamNames is not null)
            {
                // Select only teams with specific team names
                TeamQuery = TeamQuery.WhereIn(nameof(TimelineTeam.TeamName), FilteredTeamNames);
            }

            var Teams = await TeamQuery.GetAsync<CompanyTeamRestModel>();

            if (Teams.Any() is false)
                return Teams;

            StudentQuery ??= await this.Database.Collection("Timeline")
                                                .Document("Collections")
                                                .Collection("Students")
                                                .WhereIn(nameof(TimelineStudent.TeamId), Teams.Select(x => x.TeamId))
                                                .GetAsync<TimelineStudent>();

            foreach (var Team in Teams)
                Team.Students = StudentQuery.Where(x => x.TeamId == Team.TeamId).ToList();

            return Teams;

        }

        public async Task<TimelineStudent> CreateStudent(CreateTimelineStudentRequest Request)
        {

            var NewStudent = new TimelineStudent
            {
                StudentId = Guid.NewGuid().ToString(),
                FullName = Request.FullName,
                Role = Request.Role,
                DisplayPicture = Request.DisplayPicture,
                RemarkableAchievements = Request.RemarkableAchievements,
                AreaOfSpecialization = Request.AreaOfSpecialization,
                LinkedInProfile = Request.LinkedInProfile,
            };

            // If TeamName or TeamTrimester is specified
            if (Request.TeamName is not null || Request.TeamTrimester is not null)
            {

                // Team name and team trimester must both be filled or empty.
                // One cannot exist without the other.
                // We use an xor operator to check this.
                if (Request.TeamName is null ^ Request.TeamTrimester is null)
                {
                    throw new ArgumentNullException($"Both { nameof(CreateTimelineStudentRequest.TeamName) } and {nameof(CreateTimelineStudentRequest.TeamTrimester)} must be specified", nameof(Request.TeamName));
                }

                var StudentsTeam = await this.Database.Collection("Timeline")
                                                      .Document("Collections")
                                                      .Collection("Teams")
                                                      .WhereEqual(nameof(TimelineTeam.Trimester), Request.TeamTrimester)
                                                      .WhereEqual(nameof(TimelineTeam.TeamName), Request.TeamName)
                                                      .Limit(1)
                                                      .GetAsync<TimelineTeam>()
                                                      .ConfigureAwait(false);

                if (StudentsTeam.IsNullOrEmpty())
                    throw new ArgumentException("There's no team name matching ", nameof(TimelineTeam.TeamName));

                NewStudent.TeamId = StudentsTeam.First().TeamId;

            }



            var Result = await this.Database.Collection("Timeline")
                                            .Document("Collections")
                                            .Collection("Students")
                                            .Document(NewStudent.StudentId)
                                            .CreateDocumentAsync(NewStudent)
                                            .ConfigureAwait(false);

            this.Cache.Set($"TimelineStudent-{NewStudent.StudentId}", NewStudent, _timelinePersonCacheOptions);

            return NewStudent;

        }

        public async Task<TimelineStudent?> GetStudent(FindStudentRequest Request)
        {

            if (Request.StudentId is not null)
            {

                if (this.Cache.TryGetValue($"TimelineStudent-{Request.StudentId}", out TimelineStudent Student))
                    return Student;
                else
                    return await this.Database.Collection("Timeline")
                                              .Document("Collections")
                                              .Collection("Students")
                                              .Document(Request.StudentId)
                                              .GetDocumentAsync<TimelineStudent>()
                                              .ConfigureAwait(false);
            }

            else if (Request.StudentName is not null)
                return (await this.Database.Collection("Timeline")
                                              .Document("Collections")
                                              .Collection("Students")
                                              .WhereEqual(nameof(TimelineStudent.FullName), Request.StudentName)
                                              .GetAsync<TimelineStudent>()
                                              .ConfigureAwait(false))
                                              .FirstOrDefault();

            throw new ArgumentException("GetStudentRequest must have one field filled out", nameof(Request));

        }

        public async Task<TimelineStudent?> DeleteStudent(string StudentId)
        {

            this.Cache.Remove($"TimelineStudent-{StudentId}");

            return await this.Database.Collection("Timeline")
                                      .Document("Collections")
                                      .Collection("Students")
                                      .Document(StudentId)
                                      .DeleteDocumentAsync<TimelineStudent>();

        }

    }

}
