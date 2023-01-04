namespace Users.Interface.Models
{

    /// <summary>
    /// All mutable information about a Gopher Industries user.
    /// </summary>
    public interface IGopherUserData
    {

        /// <summary>
        /// The display name of the user, the name they want other people to see
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        /// The email associated with the account
        /// </summary>
        public string Email { get; init; }

        /// <summary>
        /// The organisation associated with the account
        /// </summary>
        public string Organisation { get; init; }

        /// <summary>
        /// Whether the user's email has been verified or not
        /// </summary>
        public bool? EmailVerified { get; init; }

        /// <summary>
        /// The date of birth of the user
        /// </summary>
        public DateTime DateOfBirth { get; init; }

        /// <summary>
        /// Calculate the users age from their date of birth
        /// </summary>
        public TimeSpan Age() => DateTime.Now - DateOfBirth;

    }
}
