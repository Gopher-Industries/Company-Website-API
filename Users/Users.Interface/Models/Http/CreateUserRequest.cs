using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Interface.Models.Http
{
    public record CreateUserRequest
    {

        /// <summary>
        /// The username of the person
        /// </summary>
        /// <example>Nat</example>
        public string Username { get; init; }

        /// <summary>
        /// The name to display of the person.
        /// </summary>
        /// <example>Nat</example>
        public string DisplayName { get; init; }

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
