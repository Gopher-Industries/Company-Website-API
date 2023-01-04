namespace Users.Interface.Models
{
    internal record GopherUser : GopherUserData, IGopherUser
    {
        
        public string UserId { get; init; }

        public string Username { get; init; }
        
        public DateTime Created { get; init; }

    }
}
