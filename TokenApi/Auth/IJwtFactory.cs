using nH.Identity.Core;

namespace TokenApi.Auth
{
    public interface IJwtFactory
    {
        LoginResponseData GenerateEncodedToken(User user);
    }
}