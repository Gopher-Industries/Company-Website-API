using Google.Cloud.Dialogflow.V2;
using System.Text.Json;
using SessionsClient = Google.Cloud.Dialogflow.V2.SessionsClient;
using ContextsClient = Google.Cloud.Dialogflow.V2.ContextsClient;
using AgentClient = Google.Cloud.Dialogflow.V2.AgentsClient;
using Microsoft.Extensions.Caching.Memory;
using static Google.Cloud.Dialogflow.V2.Contexts;
using ProjectX.WebAPI.Models.Chatbot;
using ProjectX.WebAPI.Models.Config;

namespace ProjectX.WebAPI.Services
{
    public interface IDialogFlowService
    {
        public Task<string> SendMessage(ChatbotMessage Message);
    }

    public class DialogFlowService : IDialogFlowService
    {

        /// <summary>
        /// Values come from https://cloud.google.com/dialogflow/es/docs/reference/rest/v2-overview
        /// </summary>
        private const string Region = "global";

        /// <summary>
        /// A way to store conversations with users for 24 hours. 
        /// I havent entirely worked it out yet.
        /// </summary>
        private readonly ConversationsClient ConversationAgent;

        /// <summary>
        /// The client we use to actually talk to the chatbot
        /// </summary>
        private readonly SessionsClient SessionAgent;

        /// <summary>
        /// Helps us inject parameters into the conversation. See here on how to use:
        /// https://cloud.google.com/dialogflow/es/docs/contexts-manage#api
        /// </summary>
        private readonly ContextsClient ContextAgent;

        /// <summary>
        /// The Id of the google project
        /// </summary>
        private readonly string ProjectId;

        /// <summary>
        /// The cache we use to store data so that we don't need to hit google cloud all the time
        /// </summary>
        private readonly IMemoryCache Cache;

        private readonly MemoryCacheEntryOptions _sessionCacheOptions = new MemoryCacheEntryOptions()
        {
            Size = 500, 
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Sessions last for 30 minutes in google cloud.
        };

        public DialogFlowService(IMemoryCache Cache,
                                 IConfiguration Config)
        {

            this.Cache = Cache;

            var config = Config.GetJson("Credentials:DialogflowAccess");

            this.ProjectId = Config["Credentials:DialogflowAccess:project_id"];

            (this.ConversationAgent, this.SessionAgent, this.ContextAgent) = this.InitializeDialogflowConnection(config).ConfigureAwait(false).GetAwaiter().GetResult();
            
        }

        /// <summary>
        /// Builds the three dialogflow endpoint clients 
        /// </summary>
        /// <returns></returns>
        private async Task<(ConversationsClient, SessionsClient, ContextsClient)> InitializeDialogflowConnection(string DialogflowAccessJson)
        {

            var ConversationClient = new ConversationsClientBuilder()
            {
                JsonCredentials = DialogflowAccessJson,
                Endpoint = $"{Region}-dialogflow.googleapis.com:443"
            }.BuildAsync();

            var SessionClient = new SessionsClientBuilder
            {
                JsonCredentials = DialogflowAccessJson,
                Endpoint = $"{Region}-dialogflow.googleapis.com:443"
            }.BuildAsync();

            var ContextClient = new ContextsClientBuilder
            {
                JsonCredentials = DialogflowAccessJson,
                Endpoint = $"{Region}-dialogflow.googleapis.com:443"
            }.BuildAsync();

            Task.WaitAll(new Task[] { ConversationClient, SessionClient, ContextClient });

            return (await ConversationClient, await SessionClient, await ContextClient);

        }

        private async Task<Context?> GetSessionContext(string Session)
        {
            
            //
            // Check if the session has been cached
            if (this.Cache.TryGetValue(Session, out Context LocalContext))
                return LocalContext;

            try
            {

                //
                // See if the session exists in google cloud

                var ServerContext = await this.ContextAgent.GetContextAsync(
                $"projects/{this.ProjectId}/locations/{Region}/agent/sessions/{Session}/contexts/4928d617-8d8d-491f-80a4-36dc7226446f_id_dialog_context")
                .ConfigureAwait(false);

                this.Cache.Set(Session, ServerContext, _sessionCacheOptions);

                return ServerContext;

            }
            catch
            {
                return null;
            }

        }


        public async Task<string> SendMessage(ChatbotMessage Message)
        {

            //
            // Find the session and check if it has been initiated before
            //

            var Context = await this.GetSessionContext(Message.Session).ConfigureAwait(false);

            var Payload = new DetectIntentRequest()
            {
                Session = $"projects/{this.ProjectId}/locations/{Region}/agent/sessions/{Message.Session}",
                QueryInput = new QueryInput()
                {
                    Text = new TextInput()
                    {
                        Text = Message.Message,
                        LanguageCode = "en-US",
                    }
                },
                QueryParams = new QueryParameters()
            };

            var FirstMessage = await this.SessionAgent.DetectIntentAsync(Payload).ConfigureAwait(false);

            var ResultResponse = String.Join(System.Environment.NewLine, FirstMessage.QueryResult.FulfillmentMessages.SelectMany(x => x.Text.Text_.ToList()).ToList());

            if (ResultResponse.Contains("your patient id", StringComparison.OrdinalIgnoreCase))
            {

                var HeaderPayload = new DetectIntentRequest()
                {
                    Session = $"projects/{this.ProjectId}/locations/{Region}/agent/sessions/{Message.Session}",
                    QueryInput = new QueryInput()
                    {
                        Text = new TextInput()
                        {
                            Text = Message.UserId,
                            LanguageCode = "en-US",
                        }
                    }
                };

                // Send off the first message as an identifying message
                // to tell dialogflow who they're dealing with.
                var SecondMessage = await this.SessionAgent.DetectIntentAsync(HeaderPayload).ConfigureAwait(false);

                this.Cache.Set(Message.Session, SecondMessage.QueryResult.Action, _sessionCacheOptions);

                return String.Join(System.Environment.NewLine, SecondMessage.QueryResult.FulfillmentMessages.SelectMany(x => x.Text.Text_.ToList()).ToList()); ;

            }

            return ResultResponse;

            //var EventPayload = new EventInput()
            //{
            //    Name = "Login",
            //    Parameters = new Google.Protobuf.WellKnownTypes.Struct
            //    {

            //    }
            //};

            //EventPayload.Parameters.Fields.Add("UserId", new Google.Protobuf.WellKnownTypes.Value
            //{
            //    StringValue = Message.UserId
            //});



            //Payload.QueryParams.Payload.Fields.Add(new Dictionary<string, Google.Protobuf.WellKnownTypes.Value>()
            //{
            //    { 
            //        "User_name", 
            //        new Google.Protobuf.WellKnownTypes.Value()
            //        {
            //            StringValue = Message.UserId
            //        }
            //    }
            //});

            //Payload.QueryParams.Contexts.Add(new Context()
            //{
            //    Name = $"projects/appointment-scheduler-med-gbwq/locations/{Region}/agent/sessions/{Message.Session}/contexts/eb9bec53-6a98-401a-baec-2247ccdb1a10_id_dialog_context",
            //    LifespanCount = 2,
            //    Parameters = new Google.Protobuf.WellKnownTypes.Struct()
            //    {

            //    }
            //});

            //Payload.QueryParams.Contexts.First().Parameters.Fields.Add(new Dictionary<string, Google.Protobuf.WellKnownTypes.Value>()
            //{
            //    {
            //        "Patient_ID",
            //        new Google.Protobuf.WellKnownTypes.Value()
            //        {
            //            StringValue = Message.UserId
            //        }
            //    }
            //});

            //Payload.QueryParams.Contexts.First().Parameters.Fields.Add(new Dictionary<string, Google.Protobuf.WellKnownTypes.Value>()
            //{
            //    {
            //        "Patient_ID.original",
            //        new Google.Protobuf.WellKnownTypes.Value()
            //        {
            //            StringValue = Message.UserId
            //        }
            //    }
            //});

            //var Query = new QueryInput()
            //{
            //    Event = EventPayload
            //};



        }

    }

}
