using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectX.WebAPI.Models.Chatbot;
using ProjectX.WebAPI.Services;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace ProjectX.WebAPI.Controllers
{
    [Route("api/v1/chatbot")]
    [ApiController]
    [Authorize]
    [SwaggerTag(description: "<h3>Medi Chatbot Endpoint</h3>")]
    public class ChatbotController : ControllerBase
    {

        private readonly IDialogFlowService botService;
        private readonly ITokenService tokenService;

        public ChatbotController(IDialogFlowService BotService,
                                 ITokenService TokenService)
        {
            botService = BotService;
            tokenService = TokenService;
        }

        /// <summary>
        /// Users can send messages to our medi chatbot through this endpoint.
        /// </summary>
        /// <returns></returns>
        [HttpPost("send")]
        public async Task<ObjectResult> Send([FromBody] ChatbotMessage Message)
        {

            var AccessToken = this.tokenService.ReadAccessToken(this.HttpContext.User);

            //var User = await database.GetUser(new FindUserRequest { Username = Request.Username }).ConfigureAwait(false);

            Message.Session ??= Guid.NewGuid().ToString();
            Message.UserId = AccessToken.UserId;

            var ResponseMessage = await botService.SendMessage(Message);

            return Ok(value: new ChatbotMessage
            {
                Session = Message.Session,
                Message = ResponseMessage,
                UserId = Message.UserId
            });

        }
    }
}
