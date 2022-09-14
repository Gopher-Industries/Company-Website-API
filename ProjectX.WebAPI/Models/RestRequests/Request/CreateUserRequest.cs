namespace ProjectX.WebAPI.Models.RestRequests.Request
{
    public record CreateUserRequest
    {

        /// <summary>
        /// The username of the person
        /// </summary>
        /// <example>Nat</example>
        public string Username { get; init; }

        /// <summary>
        /// The user's plain text password
        /// </summary>
        public string Password { get; init; }

        /// <summary>
        /// The email to register under
        /// </summary>
        /// <example>deakinstudent@deakin.edu.au</example>
        public string Email { get; init; }

        /// <summary>
        /// The date of birth for the user
        /// </summary>
        public DateTime DateOfBirth { get; init; }

        /// <summary>
        /// The organisation of the user
        /// </summary>
        /// <example>Gopher Industries</example>
        public string OrganisationName { get; init; }

    }
}
