using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectX.WebAPI.Models.Chatbot;
using ProjectX.WebAPI.Models.Database.Timeline;
using ProjectX.WebAPI.Models.RestRequests.Request;
using ProjectX.WebAPI.Models.RestRequests.Request.Timeline;
using ProjectX.WebAPI.Models.RestRequests.Response;
using ProjectX.WebAPI.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace ProjectX.WebAPI.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/v1/timeline")]
    public class TimelineController : ControllerBase
    {

        private readonly ITimelineService TimelineService;

        public TimelineController(ITimelineService TimelineService)
        {
            this.TimelineService = TimelineService;
        }

        /// <summary>
        /// Users can retrieve information about the complete timeline from this endpoint
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [SwaggerResponse(StatusCodes.Status200OK, description: "Successfully retrieved the information for the timeline", typeof(CompanyTeamRestModel))]
        public async Task<ObjectResult> GetTimeline([FromQuery] GetTimelineRequest Request)
        {

            return Ok(value: await TimelineService.GetTimeline(Request).ConfigureAwait(false));

        }

        /// <summary>
        /// Create a new student in the timeline
        /// </summary>
        /// <returns></returns>
        //[Authorize]
        [HttpPost("students/create")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The student was created successfully", typeof(TimelineStudent))]
        public async Task<ObjectResult> CreateStudent([FromBody] CreateTimelineStudentRequest Request)
        {

            try
            {
                return Ok(value: await TimelineService.CreateStudent(Request).ConfigureAwait(false));
            }
            catch (ArgumentException Ex)
            {
                return BadRequest(Ex.Message);
            }

        }

        /// <summary>
        /// Retrieve a student from the timeline
        /// </summary>
        /// <returns></returns>
        [HttpGet("students/{StudentId}")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "Successfully found the student", typeof(TimelineStudent))]
        [SwaggerResponse(StatusCodes.Status404NotFound, description: "Student was not found")]
        public async Task<ObjectResult> GetStudent([FromRoute] string StudentId)
        {

            var Student = await TimelineService.GetStudent(new FindTimelineStudentRequest { StudentId = StudentId }).ConfigureAwait(false);

            // Return Ok or 404 depending on whether the student exists
            return Student is not null ?
                Ok(value: Student) :
                NotFound(value: $"Student with ID '{StudentId}' was not found.");

        }

        /// <summary>
        /// Delete a student from the timeline
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpDelete("students/{StudentId}")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "Successfully deleted the student", typeof(TimelineStudent))]
        [SwaggerResponse(StatusCodes.Status404NotFound, description: "Student was not found")]
        public async Task<ObjectResult> DeleteStudent([FromRoute] string StudentId)
        {

            var DeletedStudent = await TimelineService.DeleteStudent(StudentId).ConfigureAwait(false);

            // Return Ok or 404 depending on whether the student exists
            return DeletedStudent is not null ?
                Ok(value: DeletedStudent) :
                NotFound(value: $"Student with ID '{StudentId}' was not found.");

        }

        /// <summary>
        /// Add a team for the timeline
        /// </summary>
        /// <returns></returns>
        //[Authorize]
        [HttpPost("teams/create")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The student was created successfully", typeof(TimelineStudent))]
        public async Task<ObjectResult> CreateTeam([FromBody] CreateTimelineTeamRequest Request)
        {

            try
            {
                return Ok(value: await TimelineService.CreateTeam(Request).ConfigureAwait(false));
            }
            catch (ArgumentException Ex)
            {
                return BadRequest(Ex.Message);
            }

        }

        /// <summary>
        /// Retrieve a team from the timeline
        /// </summary>
        /// <returns></returns>
        [HttpGet("teams/{TeamId}")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "Successfully found the student", typeof(TimelineStudent))]
        [SwaggerResponse(StatusCodes.Status404NotFound, description: "Student was not found")]
        public async Task<ObjectResult> GetTeam([FromRoute] string TeamId)
        {
            
            var Team = await TimelineService.GetTeam(TeamId).ConfigureAwait(false);

            // Return Ok or 404 depending on whether the team exists
            return Team is not null ?
                Ok(value: Team) :
                NotFound(value: $"Team with ID '{TeamId}' was not found.");

        }

        /// <summary>
        /// Find a team from the timeline by their trimester and their team name
        /// </summary>
        /// <returns></returns>
        [HttpGet("teams/{Trimester}/{TeamName}")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "Successfully found the student", typeof(TimelineStudent))]
        [SwaggerResponse(StatusCodes.Status404NotFound, description: "Student was not found")]
        public async Task<ObjectResult> FindTeam([FromRoute] string Trimester, [FromRoute] string TeamName)
        {

            var Team = await TimelineService.FindTeam(new FindTimelineTeamRequest 
            { 
                TeamName = TeamName, 
                Trimester = Trimester 
            }).ConfigureAwait(false);

            // Return Ok or 404 depending on whether the team exists
            return Team is not null ?
                Ok(value: Team) :
                NotFound(value: $"Team '{ TeamName }' in trimester '{ Trimester }' was not found.");

        }

        /// <summary>
        /// Retrieve a team from the timeline by their team id
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpDelete("team/{TeamId}")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "Successfully deleted the student", typeof(TimelineStudent))]
        [SwaggerResponse(StatusCodes.Status404NotFound, description: "Student was not found")]
        public async Task<ObjectResult> DeleteTeam([FromRoute] string TeamId)
        {

            var DeletedTeam = await TimelineService.DeleteTeam(TeamId).ConfigureAwait(false);

            // Return Ok or 404 depending on whether the student exists
            return DeletedTeam is not null ?
                Ok(value: DeletedTeam) :
                NotFound(value: $"Student with ID '{DeletedTeam}' was not found.");

        }

    }
}
