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

        public Task<IReadOnlyList<CompanyTeamRestModel>> GetTimeline(GetTimelineRequest Request);

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
        public Task<TimelineStudent?> GetStudent(FindTimelineStudentRequest Request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public Task<TimelineStudent?> DeleteStudent(string StudentId);

        /// <summary>
        /// Creates a new student in the timeline
        /// </summary>
        /// <param name="Request"></param>
        /// <exception cref="ArgumentException"/>
        /// <returns></returns>
        public Task<TimelineTeam> CreateTeam(CreateTimelineTeamRequest Request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public Task<TimelineTeam?> GetTeam(string TeamId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public Task<TimelineTeam?> FindTeam(FindTimelineTeamRequest Request);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<TimelineTeam?> DeleteTeam(string TeamId);

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

        public async Task<IReadOnlyList<CompanyTeamRestModel>> GetTimeline(GetTimelineRequest Request)
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
                    FilteredTeamNames = StudentQuery.SelectMany(x => x.Teams)
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
                                                .WhereArrayContainsAny(nameof(TimelineStudent.Teams), Teams.Select(x => x.TeamId))
                                                .GetAsync<TimelineStudent>();

            foreach (var Team in Teams)
                Team.Students = StudentQuery.Where(x => x.Teams.Contains(Team.TeamId)).ToList();

            return Teams;

        }

        public async Task<TimelineStudent> CreateStudent(CreateTimelineStudentRequest Request)
        {

            var NewStudent = new TimelineStudent
            {
                StudentId = Guid.NewGuid().ToString(),
                FullName = Request.FullName,
                Role = Request.Role,
                ProfilePicture = Request.ProfilePicture,
                RemarkableAchievements = Request.RemarkableAchievements,
                AreaOfSpecialization = Request.AreaOfSpecialization,
                LinkedInProfile = Request.LinkedInProfile,
            };

            // If TeamName or TeamTrimester is specified
            if (Request.Teams?.Any() is true)
            {

                NewStudent.Teams = Request.Teams.AsParallel().Select(async Team =>
                {

                    var StudentsTeam = await this.Database.Collection("Timeline")
                                                          .Document("Collections")
                                                          .Collection("Teams")
                                                          .WhereEqual(nameof(Team.Trimester), Team.Trimester)
                                                          .WhereEqual(nameof(Team.TeamName), Team.TeamName)
                                                          .Limit(1)
                                                          .GetAsync<TimelineTeam>()
                                                          .ConfigureAwait(false);

                    if (StudentsTeam.IsNullOrEmpty())
                        throw new ArgumentException("There's no team name matching ", nameof(Team.TeamName));

                    return StudentsTeam.First().TeamId;

                }).ToArray().Select(x => x.ConfigureAwait(false).GetAwaiter().GetResult()).ToArray();

            }

            var CreateTimelineStudentResult = await this.Database.Collection("Timeline")
                                                                 .Document("Collections")
                                                                 .Collection("Students")
                                                                 .Document(NewStudent.StudentId)
                                                                 .CreateDocumentAsync(NewStudent)
                                                                 .ConfigureAwait(false);

            this.Cache.Set($"TimelineStudent-{NewStudent.StudentId}", NewStudent, _timelinePersonCacheOptions);

            return NewStudent;

        }

        public async Task<TimelineStudent?> GetStudent(FindTimelineStudentRequest Request)
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

        public async Task<TimelineTeam> CreateTeam(CreateTimelineTeamRequest Request)
        {

            //
            // Validate 

            var TimelineTeam = new TimelineTeam
            {
                TeamId = Guid.NewGuid().ToString(),
                Description = Request.Description,
                Logo = Request.Logo,
                Mentors = Request.Mentors,
                PrototypeLink = Request.PrototypeLink,
                TeamName = Request.TeamName,
                Trimester = Request.Trimester,
                VideoLink = Request.VideoLink
            };

            return await this.Database.Collection("Timeline")
                                      .Document("Collections")
                                      .Collection("Teams")
                                      .Document(TimelineTeam.TeamId)
                                      .SetDocumentAsync<TimelineTeam>(TimelineTeam);

        }

        public async Task<TimelineTeam?> GetTeam(string TeamId)
        {

            return await this.Database.Collection("Timeline")
                                      .Document("Collections")
                                      .Collection("Teams")
                                      .Document(TeamId)
                                      .GetDocumentAsync<TimelineTeam>();

        }

        public async Task<TimelineTeam?> FindTeam(FindTimelineTeamRequest Request)
        {

            return (await this.Database.Collection("Timeline")
                                       .Document("Collections")
                                       .Collection("Teams")
                                       .WhereEqual(nameof(TimelineTeam.TeamName), Request.TeamName)
                                       .WhereEqual(nameof(TimelineTeam.Trimester), Request.Trimester)
                                       .GetAsync<TimelineTeam>()
                                       .ConfigureAwait(false))
                                       .FirstOrDefault();

        }

        public async Task<TimelineTeam?> DeleteTeam(string TeamId)
        {

            return await this.Database.Collection("Timeline")
                                      .Document("Collections")
                                      .Collection("Teams")
                                      .Document(TeamId)
                                      .DeleteDocumentAsync<TimelineTeam>();

        }

    }

}
