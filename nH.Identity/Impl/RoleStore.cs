using Microsoft.AspNetCore.Identity;
using nH.Identity.Core;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nH.Identity.Impl
{
    public class RoleStore : IQueryableRoleStore<Role>, IRoleClaimStore<Role>
    {
        private readonly ISession _session;

        public RoleStore(ISession session)
        {
            this._session = session;
        }

        System.Linq.IQueryable<Role> IQueryableRoleStore<Role>.Roles => _session.Query<Role>();

        Task IRoleClaimStore<Role>.AddClaimAsync(Role role, Claim claim, CancellationToken cancellationToken)
        {
            var roleClaim = new RoleClaim { ClaimType = claim.Type, ClaimValue = claim.Value, Role = role };
            role.RoleClaims.Add(roleClaim);
            return Task.FromResult<object>(null);
        }

        async Task<IdentityResult> IRoleStore<Role>.CreateAsync(Role role, CancellationToken cancellationToken)
        {
            await _session.SaveAsync(role, cancellationToken);
            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<Role>.DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            await _session.DeleteAsync(role, cancellationToken);
            return IdentityResult.Success;
        }

        void IDisposable.Dispose()
        {
            //noop
        }

        async Task<Role> IRoleStore<Role>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var id = long.Parse(roleId);
            return await _session.GetAsync<Role>(id);
        }

        async Task<Role> IRoleStore<Role>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await _session.Query<Role>().Where(q => q.Name == normalizedRoleName).SingleOrDefaultAsync();
        }

        Task<IList<Claim>> IRoleClaimStore<Role>.GetClaimsAsync(Role role, CancellationToken cancellationToken)
        {
            IList<Claim> claims = role.RoleClaims.Select(q => new Claim(q.ClaimType, q.ClaimValue)).ToList();
            return Task.FromResult(claims);
        }

        Task<string> IRoleStore<Role>.GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        Task<string> IRoleStore<Role>.GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        Task<string> IRoleStore<Role>.GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        async Task IRoleClaimStore<Role>.RemoveClaimAsync(Role role, Claim claim, CancellationToken cancellationToken)
        {
            var removedClaims = role.RoleClaims.Where(q => q.ClaimType == claim.Type && q.ClaimValue == claim.Value).ToList();
            foreach (var removedClaim in removedClaims)
            {
                role.RoleClaims.Remove(removedClaim);
            }
            await _session.FlushAsync(cancellationToken);
        }

        Task IRoleStore<Role>.SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            role.Name = normalizedName;
            return Task.FromResult<object>(null);
        }

        Task IRoleStore<Role>.SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.FromResult<object>(null);
        }

        async Task<IdentityResult> IRoleStore<Role>.UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            await _session.UpdateAsync(role, cancellationToken);
            return IdentityResult.Success;
        }
    }
}
