using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectX.WebAPI.Models.Chatbot;
using ProjectX.WebAPI.Models.RestRequests.Request;
using ProjectX.WebAPI.Services;

namespace ProjectX.WebAPI.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/v1/timeline")]
    public class TimelineController : ControllerBase
    {

        private readonly ITimelineService TimelineService;
        private readonly HttpClient client;

        public TimelineController(ITimelineService TimelineService, HttpClient Client)
        {
            this.TimelineService = TimelineService;
            client = Client;
        }

        /// <summary>
        /// Users can send messages to our medi chatbot through this endpoint.
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public async Task<ObjectResult> Get([FromQuery] TimelineRequest Request)
        {

            var response = await this.client.GetAsync("https://firestore.googleapis.com/v1/projects/prototypeprojectx/databases/(default)/documents/Users/2acbf664-3915-43ae-b988-631a9bc16580");
            Console.WriteLine(response);

            return Ok(value: await TimelineService.GetTimeline(Request));

        }

        /// <summary>
        /// Users can send messages to our medi chatbot through this endpoint.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/students/{StudentId}")]
        public async Task<ObjectResult> Get([FromQuery] string StudentId)
        {

            var Request = new TimelineRequest
            {
                StudentId = StudentId
            };

            return Ok(value: await TimelineService.GetTimeline(Request));

        }

    }
}
