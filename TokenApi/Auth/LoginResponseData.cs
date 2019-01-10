namespace TokenApi.Auth
{
    public class LoginResponseData
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
        public bool isAdmin { get; set; }
    }
}