namespace Users.Interface.Models
{

    /// <summary>
    /// Represents a Gopher Industries user
    /// </summary>
    public interface IGopherUser : IGopherUserData
    {

        /// <summary>
        /// The user's unique Id
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// The user's unique username
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// The date that the user has created their account
        /// </summary>
        public DateTime Created { get; }

    }
}
