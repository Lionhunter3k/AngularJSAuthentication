using nH.Identity.Core;
using System.Threading.Tasks;
using TokenApi.Entities;

namespace TokenApi.Auth
{
    public interface IJwtFactory
    {
        Task<LoginResponseData> GenerateEncodedTokenAsync(User user, string refreshTokenType);
        Task<User> GetUserAsync(string refreshTokenValue);
    }
}