using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using nH.Identity.Core;
using nH.Identity.Impl;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nH.Identity.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IdentityBuilder RegisterSessionStores(this IdentityBuilder builder)
        {
            if(builder.UserType != typeof(User))
            {
                throw new NotSupportedException("RoleType must be of nH.Identity.User, nH.Identity type");
            }
            builder.Services.AddScoped(typeof(IUserStore<User>), typeof(UserStore));
            if (builder.RoleType != null)
            {
                if(builder.RoleType != typeof(Role))
                {
                    throw new NotSupportedException("RoleType must be of nH.Identity.Role, nH.Identity type");
                }
                builder.Services.AddScoped(typeof(IRoleStore<Role>), typeof(RoleStore));
            }
            return builder;
        }
    }
}
