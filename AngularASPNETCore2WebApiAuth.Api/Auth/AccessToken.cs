namespace AngularASPNETCore2WebApiAuth.Api.Auth
{
    public sealed class AccessToken
    {
        public string Value { get; }
        public int ExpiresInSeconds { get; }

        public AccessToken(string value, int expiresInSeconds)
        {
            Value = value;
            ExpiresInSeconds = expiresInSeconds;
        }
    }
}