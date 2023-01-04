namespace ProjectX.WebAPI.Models.Chatbot
{
    public record ChatbotMessage
    {

        /// <summary>
        /// The message to send to our chatbot
        /// </summary>
        /// <example>I hurt myself running!</example>
        public string Message { get; set; }

        /// <summary>
        /// The Id of the session this conversation relates to. 
        /// Leave blank for new sessions.
        /// </summary>
        /// <example>478b7219-78c0-42b4-83f7-4973020d72df</example>
        public string Session { get; set; }

        /// <summary>
        /// The user's Id that the message is coming from.
        /// The reason this is internal is we don't want this coming from the REST request.
        /// This should be internal use only within the application.
        /// </summary>
        /// <example>98451cba-45c6-4a4b-a450-bf7022e1b8d5</example>
        internal string UserId { get; set; }

    }
}
