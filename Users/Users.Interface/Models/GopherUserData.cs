namespace Users.Interface.Models
{
    internal record GopherUserData : IGopherUserData
    {

        public string DisplayName { get; init; }

        public string Email { get; init; }

        public string Organisation { get; init; }

        public bool? EmailVerified { get; init; }

        public DateTime DateOfBirth { get; init; }

    }
}
