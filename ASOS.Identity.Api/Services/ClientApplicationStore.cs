using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASOS.Identity.Api.Entities;
using NHibernate;

namespace ASOS.Identity.Api.Services
{
    public class ClientApplicationStore : IClientApplicationStore
    {
        private readonly ISession _session;

        public ClientApplicationStore(ISession session)
        {
            this._session = session;
        }

        public async Task<ClientApplication> GetClientApplicationAsync(string clientId, CancellationToken cancellationToken = default)
        {
            return await _session.GetAsync<ClientApplication>(clientId, cancellationToken);
        }
    }
}
