using ASOS.Identity.Api.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASOS.Identity.Api.Services
{
    public interface IClientApplicationStore
    {
        Task<ClientApplication> GetClientApplicationAsync(string clientId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<string>> GetAllAllowedRedirectUrisAsync();
    }
}
