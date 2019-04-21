using Microsoft.AspNetCore.Identity;
using nH.Identity.Core;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nH.Identity.Impl
{
    public class UserStore :
        IUserLoginStore<User>,
        IUserRoleStore<User>,
        IUserClaimStore<User>,
        IUserPasswordStore<User>,
        IUserSecurityStampStore<User>,
        IUserEmailStore<User>,
        IUserLockoutStore<User>,
        IUserPhoneNumberStore<User>,
        IQueryableUserStore<User>,
        IUserTwoFactorStore<User>,
        IUserAuthenticationTokenStore<User>,
        IUserAuthenticatorKeyStore<User>
    {
        private readonly ISession _session;

        public UserStore(ISession session)
        {
            this._session = session;
        }

        private UserToken InternalGetToken(User user, string loginProvider, string name)
        {
            var entry = user.UserTokens
                .Where(_ => _.User == user && _.LoginProvider == loginProvider && _.Name == name)
                .FirstOrDefault();

            return entry;
        }

        private async Task InternalSetTokenAsync(User user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            var entry = user.UserTokens
                .Where(_ => _.User == user && _.LoginProvider == loginProvider && _.Name == name)
                .FirstOrDefault();

            if(entry != null)
            {
                entry.Value = value;
                await _session.UpdateAsync(entry, cancellationToken);
            }
            else
            {
                user.UserTokens.Add(new UserToken { LoginProvider = loginProvider, Name = name, Value = value, User = user });
                await _session.FlushAsync(cancellationToken);
            }
        }

        IQueryable<User> IQueryableUserStore<User>.Users => _session.Query<User>().Where(q => q.Deleted == false);

        void IDisposable.Dispose()
        {
            //noop;
        }

        Task IUserClaimStore<User>.AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var claim in claims)
            {
                var userClaim = new UserClaim { ClaimType = claim.Type, ClaimValue = claim.Value, User = user };
                user.UserClaims.Add(userClaim);
            }
            return Task.FromResult<object>(null);
        }

        async Task IUserLoginStore<User>.AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            var userLogin = new UserLogin { LoginProvider = login.LoginProvider, ProviderDisplayName = login.ProviderDisplayName, ProviderKey = login.ProviderKey, User = user };
            user.UserLogins.Add(userLogin);
            await _session.SaveAsync(userLogin, cancellationToken);
        }

        async Task IUserRoleStore<User>.AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = await _session.Query<Role>().SingleAsync(q => q.Name == roleName);
            user.Roles.Add(role);
        }

        async Task<IdentityResult> IUserStore<User>.CreateAsync(User user, CancellationToken cancellationToken)
        {
            var addr = new MailAddress(user.Email);
            user.Id = $"{addr.User.ToLowerInvariant()}_{Guid.NewGuid().ToString("n").Substring(0, 6)}";
            await _session.SaveAsync(user, cancellationToken);
            return IdentityResult.Success;
        }

        async Task<IdentityResult> IUserStore<User>.DeleteAsync(User user, CancellationToken cancellationToken)
        {
            if (user.Deleted)
                return IdentityResult.Failed(new IdentityError { Code = "Users.AlreadyDeleted", Description = "User is already deleted" });
            user.Deleted = true;
            await _session.UpdateAsync(user, cancellationToken);
            return IdentityResult.Success;
        }

        async Task<User> IUserEmailStore<User>.FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _session.Query<User>().Where(q => q.Deleted == false).Where(q => q.Email == normalizedEmail).SingleOrDefaultAsync(cancellationToken);
        }

        async Task<User> IUserStore<User>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _session.Query<User>().Where(q => q.Deleted == false).Where(q => q.Id == userId).SingleOrDefaultAsync(cancellationToken);
        }

        async Task<User> IUserLoginStore<User>.FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return await _session.Query<User>().Where(q => q.Deleted == false).Where(q => q.UserLogins.Any(t => t.LoginProvider == loginProvider && t.ProviderKey == providerKey)).SingleOrDefaultAsync(cancellationToken);
        }

        async Task<User> IUserStore<User>.FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _session.Query<User>().Where(q => q.Deleted == false).Where(q => q.DisplayName == normalizedUserName).SingleOrDefaultAsync(cancellationToken);
        }

        Task<int> IUserLockoutStore<User>.GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        Task<string> IUserAuthenticatorKeyStore<User>.GetAuthenticatorKeyAsync(User user, CancellationToken cancellationToken)
        {
            //or set it directly on the user as a 'TwoFactorAuthenticatorKey' property
            var result = InternalGetToken(user, "[AspNetUserStore]", "AuthenticatorKey");
            return Task.FromResult(result?.Value);
        }

        Task<IList<Claim>> IUserClaimStore<User>.GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            IList<Claim> claims = user.UserClaims.Select(q => new Claim(q.ClaimType, q.ClaimValue)).ToList();
            return Task.FromResult(claims);
        }

        Task<string> IUserEmailStore<User>.GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        Task<bool> IUserEmailStore<User>.GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        Task<bool> IUserLockoutStore<User>.GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        Task<DateTimeOffset?> IUserLockoutStore<User>.GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult<DateTimeOffset?>(user.LockoutEndUtc);
        }

        Task<IList<UserLoginInfo>> IUserLoginStore<User>.GetLoginsAsync(User user, CancellationToken cancellationToken)
        {
            IList<UserLoginInfo> claims = user.UserLogins.Select(q => new UserLoginInfo(q.LoginProvider, q.ProviderKey, q.ProviderDisplayName)).ToList();
            return Task.FromResult(claims);
        }

        Task<string> IUserEmailStore<User>.GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        Task<string> IUserStore<User>.GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.DisplayName);
        }

        Task<string> IUserPasswordStore<User>.GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        Task<string> IUserPhoneNumberStore<User>.GetPhoneNumberAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        Task<bool> IUserPhoneNumberStore<User>.GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        Task<IList<string>> IUserRoleStore<User>.GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            IList<string> roles = user.Roles.Select(q => q.Name).ToList();
            return Task.FromResult(roles);
        }

        Task<string> IUserSecurityStampStore<User>.GetSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        Task<string> IUserAuthenticationTokenStore<User>.GetTokenAsync(User user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return Task.FromResult(InternalGetToken(user, loginProvider, name)?.Value);
        }

        Task<bool> IUserTwoFactorStore<User>.GetTwoFactorEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        Task<string> IUserStore<User>.GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        Task<string> IUserStore<User>.GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.DisplayName);
        }

        async Task<IList<User>> IUserClaimStore<User>.GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            return await _session.Query<UserClaim>().Where(q => q.ClaimValue == claim.Value && q.ClaimType == claim.Type).Select(q => q.User).ToListAsync(cancellationToken);
        }

        async Task<IList<User>> IUserRoleStore<User>.GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            return await _session.Query<User>().Where(q => q.Roles.Any(t => t.Name == roleName)).ToListAsync(cancellationToken);
        }

        Task<bool> IUserPasswordStore<User>.HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        Task<int> IUserLockoutStore<User>.IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        Task<bool> IUserRoleStore<User>.IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Roles.Any(t => t.Name == roleName));
        }

        async Task IUserClaimStore<User>.RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach(var claim in claims)
            {
                var removedClaims = user.UserClaims.Where(q => q.ClaimType == claim.Type && q.ClaimValue == claim.Value).ToList();
                foreach(var removedClaim in removedClaims)
                {
                    user.UserClaims.Remove(removedClaim);
                }
            }
            await _session.FlushAsync(cancellationToken);
        }

        async Task IUserRoleStore<User>.RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var removedRole = user.Roles.SingleOrDefault(q => q.Name == roleName);
            if(removedRole != null)
            {
                user.Roles.Remove(removedRole);
            }
            await _session.FlushAsync(cancellationToken);
        }

        async Task IUserLoginStore<User>.RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var removedLogins = user.UserLogins.Where(q => q.LoginProvider == loginProvider && q.ProviderKey == providerKey).ToList();
            foreach (var removedLogin in removedLogins)
            {
                user.UserLogins.Remove(removedLogin);
            }
            await _session.FlushAsync(cancellationToken);
        }

        async Task IUserAuthenticationTokenStore<User>.RemoveTokenAsync(User user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var token = InternalGetToken(user, loginProvider, name);
            if(token != null)
            {
                user.UserTokens.Remove(token);
                await _session.FlushAsync(cancellationToken);
            }
        }

        async Task IUserClaimStore<User>.ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            var updatedClaims = user.UserClaims.Where(q => q.ClaimType == claim.Type && q.ClaimValue == claim.Value).ToList();
            foreach(var updatedClaim in updatedClaims)
            {
                updatedClaim.ClaimType = newClaim.Type;
                updatedClaim.ClaimValue = newClaim.Value;
            }
            await _session.FlushAsync(cancellationToken);
        }

        Task IUserLockoutStore<User>.ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return Task.FromResult<object>(null);
        }

        Task IUserAuthenticatorKeyStore<User>.SetAuthenticatorKeyAsync(User user, string key, CancellationToken cancellationToken)
        {
            return InternalSetTokenAsync(user, "[AspNetUserStore]", "AuthenticatorKey", key, cancellationToken);
        }

        Task IUserEmailStore<User>.SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult<object>(null);
        }

        Task IUserEmailStore<User>.SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            if (confirmed)
                user.EmailConfirmedOnUtc = DateTime.UtcNow;
            else
                user.EmailConfirmedOnUtc = null;
            return Task.FromResult<object>(null);
        }

        Task IUserLockoutStore<User>.SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult<object>(null);
        }

        Task IUserLockoutStore<User>.SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEndUtc = lockoutEnd?.UtcDateTime;
            return Task.FromResult<object>(null);
        }

        Task IUserEmailStore<User>.SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.Email = normalizedEmail;
            return Task.FromResult<object>(null);
        }

        Task IUserStore<User>.SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.DisplayName = normalizedName;
            return Task.FromResult<object>(null);
        }

        Task IUserPasswordStore<User>.SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult<object>(null);
        }

        Task IUserPhoneNumberStore<User>.SetPhoneNumberAsync(User user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult<object>(null);
        }

        Task IUserPhoneNumberStore<User>.SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            if (confirmed)
                user.PhoneNumberConfirmedOnUtc = DateTime.UtcNow;
            else
                user.PhoneNumberConfirmedOnUtc = null;
            return Task.FromResult<object>(null);
        }

        Task IUserSecurityStampStore<User>.SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult<object>(null);
        }

        Task IUserAuthenticationTokenStore<User>.SetTokenAsync(User user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            return InternalSetTokenAsync(user, loginProvider, name, value, cancellationToken);
        }

        Task IUserTwoFactorStore<User>.SetTwoFactorEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult<object>(null);
        }

        Task IUserStore<User>.SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.DisplayName = userName;
            return Task.FromResult<object>(null);
        }

        async Task<IdentityResult> IUserStore<User>.UpdateAsync(User user, CancellationToken cancellationToken)
        {
            await _session.UpdateAsync(user, cancellationToken);
            return IdentityResult.Success;
        }
    }
}
