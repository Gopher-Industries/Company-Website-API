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

    public Task<IReadOnlyList<TimelineStudent>> GetStudentTimeline();

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
    public Task<TimelineStudent?> DeleteStudent(string StudentTimelineId);

    /// <summary>
    ///
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

    public async Task<IReadOnlyList<TimelineStudent>> GetStudentTimeline()
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

      this.Cache.Set($"TimelineStudent-{NewStudent.StudentId}", NewStudent, _timelinePersonCacheOptions);

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
