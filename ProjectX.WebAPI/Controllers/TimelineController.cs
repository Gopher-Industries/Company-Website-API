using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectX.WebAPI.Models.Chatbot;
using ProjectX.WebAPI.Models.Database.Timeline;
using ProjectX.WebAPI.Models.RestRequests.Request;
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
        public async Task<ObjectResult> GetTimeline([FromQuery] TimelineRequest Request)
        {

            return Ok(value: await TimelineService.GetTimeline(Request).ConfigureAwait(false));

        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("students/{StudentId}")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "Successfully found the student", typeof(TimelineStudent))]
        [SwaggerResponse(StatusCodes.Status404NotFound, description: "Student was not found")]
        public async Task<ObjectResult> GetStudent([FromRoute] string StudentId)
        {

            var Student = await TimelineService.GetStudent(new FindStudentRequest { StudentId = StudentId }).ConfigureAwait(false);

            // Return Ok or 404 depending on whether the student exists
            return Student is not null ?
                Ok(value: Student) :
                NotFound(value: $"Student with ID '{StudentId}' was not found.");

        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <returns></returns>
        //[Authorize]
        [HttpPost("students/create")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "The student was created successfully", typeof(TimelineStudent))]
        public async Task<ObjectResult> CreateTeam([FromBody] CreateTimelineStudentRequest Request)
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
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("trimester/{Trimester}/team/{TeamId}")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "Successfully found the student", typeof(TimelineStudent))]
        [SwaggerResponse(StatusCodes.Status404NotFound, description: "Student was not found")]
        public async Task<ObjectResult> GetTeam([FromRoute] string Trimester, [FromRoute] string TeamName)
        {

            var Student = await TimelineService.GetStudent(new FindStudentRequest { StudentId = StudentId }).ConfigureAwait(false);

            // Return Ok or 404 depending on whether the student exists
            return Student is not null ?
                Ok(value: Student) :
                NotFound(value: $"Team with ID '{StudentId}' was not found.");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpDelete("team/{StudentId}")]
        [SwaggerResponse(StatusCodes.Status200OK, description: "Successfully deleted the student", typeof(TimelineStudent))]
        [SwaggerResponse(StatusCodes.Status404NotFound, description: "Student was not found")]
        public async Task<ObjectResult> DeleteTeam([FromRoute] string StudentId)
        {

            var DeletedStudent = await TimelineService.DeleteStudent(StudentId).ConfigureAwait(false);

            // Return Ok or 404 depending on whether the student exists
            return DeletedStudent is not null ?
                Ok(value: DeletedStudent) :
                NotFound(value: $"Student with ID '{StudentId}' was not found.");

        }

    }
}
