namespace Users.Models
{
    public record FindUserRequest
    {

        public string? UserId { get; init; }

        public string? Username { get; init; }

        public string? Email { get; init; }

    }
}
