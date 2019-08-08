namespace Anderson.Structures
{
    public struct TokenKey
    {
        public string UserId { get; }
        public string Server { get; }

        public TokenKey(string userId, string server)
        {
            UserId = userId;
            Server = server;
        }
    }
}
