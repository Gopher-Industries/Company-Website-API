using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using ProjectX.WebAPI.Models.Database.Timeline;
using ProjectX.WebAPI.Models.RestRequests.Request.Timeline;

namespace ProjectX.WebAPI.Services
{
  public interface ITimelineService
  {

    public Task<IReadOnlyList<TimelineStudent>> GetAllStudentTimelines();

    /// <summary>
    /// Creates a new student in the timeline
    /// </summary>
    /// <param name="Request"></param>
    /// <exception cref="ArgumentException"/>
    /// <returns></returns>
    public Task<TimelineStudent> CreateStudent(CreateTimelineStudentRequest Request);

    /// <summary>
    /// Retrieve a student timeline object
    /// </summary>
    /// <param name="Request"></param>
    /// <exception cref="ArgumentException"/>
    /// <returns></returns>
    public Task<TimelineStudent> GetAStudentTimeline(string TimelineStudentId);


    /// <summary>
    /// Retrieve the timelines of a particular student
    /// </summary>
    /// <param name="Request"></param>
    /// <exception cref="ArgumentException"/>
    /// <returns></returns>
    public Task<IReadOnlyList<TimelineStudent>> GetStudentTimelines(FindTimelineStudentRequest Request);

    /// <summary>
    /// Delete the student timeline entry
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    public Task<TimelineStudent?> DeleteStudent(string StudentTimelineId);

    /// <summary>
    /// Update the student timeline entry
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    public Task<TimelineStudent?> UpdateStudent(UpdateTimelineStudentRequest Request);

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

    public async Task<IReadOnlyList<TimelineStudent>> GetAllStudentTimelines()
    {
      return await this.Database.GetAllDocuments<TimelineStudent>("Timeline/Collections/Students")
                                  .ConfigureAwait(false);

    }

    public async Task<TimelineStudent> CreateStudent(CreateTimelineStudentRequest Request)
    {

      var NewStudent = new TimelineStudent
      {
        TimelineStudentId = Guid.NewGuid().ToString(),
        StudentId = Request.StudentId,
        FullName = Request.FullName,
        Date = DateTime.UtcNow,
        Title = Request.Title,
        Description = Request.Description,
      };

      var CreateTimelineStudentResult = await this.Database.Collection("Timeline")
                                                           .Document("Collections")
                                                           .Collection("Students")
                                                           .Document(NewStudent.TimelineStudentId)
                                                           .CreateDocumentAsync(NewStudent)
                                                           .ConfigureAwait(false);

      this.Cache.Set($"TimelineStudent-{NewStudent.TimelineStudentId}", NewStudent, _timelinePersonCacheOptions);

      return NewStudent;
    }

    public async Task<TimelineStudent> UpdateStudent(UpdateTimelineStudentRequest Request)
    {

      var student = await this.Database.Collection("Timeline")
                                        .Document("Collections")
                                        .Collection("Students")
                                        .Document(Request.StudentTimelineId)
                                        .GetDocumentAsync<TimelineStudent>()
                                        .ConfigureAwait(false);

      if (student != null)
      {
        // Prepare a list to store the updates
        var updates = new List<(string, object)>();

        if (Request.StudentId != null)
        {
          updates.Add(("StudentId", Request.StudentId));
        }

        if (Request.FullName != null)
        {
          updates.Add(("FullName", Request.FullName));
        }

        if (Request.Title != null)
        {
          updates.Add(("Title", Request.Title));
        }

        if (Request.Description != null)
        {
          updates.Add(("Description", Request.Description));
        }

        // Update the document in the database
        await this.Database.UpdateDocument<TimelineStudent>("Timeline/Collections/Students", Request.StudentTimelineId, updates.ToArray()).ConfigureAwait(false);

        // Get the updated student data
        var updatedStudent = await this.Database.GetDocument<TimelineStudent>("Timeline/Collections/Students", Request.StudentTimelineId).ConfigureAwait(false);
        return updatedStudent;
      }
      else
      {
        throw new ArgumentException("Student not found.");
      }
    }

    public async Task<TimelineStudent> GetAStudentTimeline(string TimelineStudentId)
    {
      if (this.Cache.TryGetValue($"TimelineStudent-{TimelineStudentId}", out TimelineStudent Student))
      {
        if (Student.TimelineStudentId == TimelineStudentId)
          return Student;
      }

      return await this.Database.Collection("Timeline")
                                        .Document("Collections")
                                        .Collection("Students")
                                        .Document(TimelineStudentId)
                                        .GetDocumentAsync<TimelineStudent>()
                                        .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TimelineStudent>> GetStudentTimelines(FindTimelineStudentRequest Request)
    {
      if (Request.StudentId != null)
      {
        var students = await this.Database.Collection("Timeline")
                                            .Document("Collections")
                                            .Collection("Students")
                                            .WhereEqual(nameof(TimelineStudent.StudentId), Request.StudentId)
                                            .GetAsync<TimelineStudent>()
                                            .ConfigureAwait(false);
        return students;
      }
      // if (Request.StudentName != null)
      // {
      //   var students = await this.Database.Collection("Timeline")
      //                                     .Document("Collections")
      //                                     .Collection("Students")
      //                                     .WhereEqual(nameof(TimelineStudent.FullName), Request.StudentName)
      //                                     .GetAsync<TimelineStudent>()
      //                                     .ConfigureAwait(false);
      //   return students;
      // }

      throw new ArgumentException("GetStudentRequest must have StudentId filled out", nameof(Request));
    }


    public async Task<TimelineStudent?> DeleteStudent(string StudentTimelineId)
    {

      this.Cache.Remove($"TimelineStudent-{StudentTimelineId}");

      return await this.Database.Collection("Timeline")
                                .Document("Collections")
                                .Collection("Students")
                                .Document(StudentTimelineId)
                                .DeleteDocumentAsync<TimelineStudent>();

    }
    
    public async Task<TimelineTeam> CreateTeam(CreateTimelineTeamRequest Request)
        {

            var NewTeam = new TimelineTeam
            {
                TimelineTeamId = Guid.NewGuid().ToString(),
                TeamId = Request.TeamId,
                TeamName = Request.TeamName,
                Date = DateTime.UtcNow,
                Title = Request.Title,
                Description = Request.Description,
            };

            var CreateTimelineTeamResult = await this.Database.Collection("Timeline")
                                                           .Document("Collections")
                                                           .Collection("Teams")
                                                           .Document(NewTeam.TimelineTeamId)
                                                           .CreateDocumentAsync(NewTeam)
                                                           .ConfigureAwait(false);

            this.Cache.Set($"TimelineTeam-{NewTeam.TeamId}", NewTeam, _timelinePersonCacheOptions);

            return NewTeam;
        }

        public async Task<TimelineTeam> UpdateTeam(UpdateTimelineTeamRequest Request)
        {
            var team = await this.Database.Collection("Timeline")
                                  .Document("Collections")
                                  .Collection("Teams")
                                  .Document(Request.TeamTimelineId)
                                  .GetDocumentAsync<TimelineTeam>()
                                  .ConfigureAwait(false);
            if (team != null)
            {
                // Prepare a list to store the updates
                var updates = new List<(string, object)>();

                if (Request.TeamId != null)
                {
                    updates.Add(("TeamId", Request.TeamId));
                }

                if (Request.TeamName != null)
                {
                    updates.Add(("TeamName", Request.TeamName));
                }

                if (Request.Title != null)
                {
                    updates.Add(("Title", Request.Title));
                }

                if (Request.Description != null)
                {
                    updates.Add(("Description", Request.Description));
                }

                // Update the document in the database
                await this.Database.UpdateDocument<TimelineTeam>("Timeline/Collections/Teams", Request.TeamTimelineId, updates.ToArray()).ConfigureAwait(false);

                // Get the updated student data
                var updatedTeam = await this.Database.GetDocument<TimelineTeam>("Timeline/Collections/Teams", Request.TeamTimelineId).ConfigureAwait(false);
                return updatedTeam;
            }
            else
            {
                throw new ArgumentException("Team not found.");
            }
        }

        public async Task<TimelineTeam?> GetTeam(FindTimelineTeamRequest Request)
        {
            if (Request.TeamId is not null)
            {

                if (this.Cache.TryGetValue($"TimelineTeam-{Request.TeamId}", out TimelineTeam Team))
                    return Team;
                else
                    return await this.Database.Collection("Timeline")
                                              .Document("Collections")
                                              .Collection("Teams")
                                              .Document(Request.TeamId)
                                              .GetDocumentAsync<TimelineTeam>()
                                              .ConfigureAwait(false);
            }

            else if (Request.TeamName is not null)
                return (await this.Database.Collection("Timeline")
                                              .Document("Collections")
                                              .Collection("Teams")
                                              .WhereEqual(nameof(TimelineTeam.TeamName), Request.TeamName)
                                              .GetAsync<TimelineTeam>()
                                              .ConfigureAwait(false))
                                              .FirstOrDefault();

            throw new ArgumentException("GetTeamRequest must have one field filled out", nameof(Request));
        }

        public async Task<TimelineTeam?> DeleteTeam(string TeamId)
        {
            this.Cache.Remove($"TimelineTeam-{TeamId}");

            return await this.Database.Collection("Timeline")
                           .Document("Collections")
                           .Collection("Teams")
                           .Document(TeamId)
                           .DeleteDocumentAsync<TimelineTeam>();
        }


                 .Collection("Teams")
                                .Document(TeamId)
                                .GetDocumentAsync<TimelineTeam>();

    }

  }

}
