namespace Authentication.Interface.Models.Tokens
{
    public interface IAccessToken
    {

        public string UserId { get; }

        public string Username { get; }

        public string Role { get; }

        public string SignedJWT { get; }

    }
}
