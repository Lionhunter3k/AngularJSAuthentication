using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Api.Infrastructure.Extensions
{
    public static class IdentityExtensions
    {
        public static IdentityBuilder AddIdentityRole<TRole>(this IdentityBuilder builder) where TRole : class
        {
            builder = new IdentityBuilder(builder.UserType, typeof(TRole), builder.Services);
            builder.Services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            var userClaimsPrincipalFactoryServiceType = typeof(IUserClaimsPrincipalFactory<>).MakeGenericType(builder.UserType);
            builder.Services.RemoveAll(userClaimsPrincipalFactoryServiceType);
            builder.Services.TryAddScoped(userClaimsPrincipalFactoryServiceType, typeof(UserClaimsPrincipalFactory<,>).MakeGenericType(builder.UserType, builder.RoleType));
            builder.Services.TryAddScoped<RoleManager<TRole>>();
            return builder;
        }
        /// <summary>
        /// Adds the default identity system configuration for the specified User and Role types. (Without Authentication Scheme)
        /// </summary>
        /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
        /// <typeparam name="TRole">The type representing a Role in the system.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <returns>An <see cref="IdentityBuilder"/> for creating and configuring the identity system.</returns>
        public static IdentityBuilder AddIdentityCore<TUser, TRole>(this IServiceCollection services)
            where TUser : class
            where TRole : class
            => services.AddIdentityCore<TUser, TRole>(setupAction: null);

        /// <summary>
        /// Adds and configures the identity system for the specified User and Role types. (Without Authentication Scheme)
        /// </summary>
        /// <typeparam name="TUser">The type representing a User in the system.</typeparam>
        /// <typeparam name="TRole">The type representing a Role in the system.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setupAction">An action to configure the <see cref="IdentityOptions"/>.</param>
        /// <returns>An <see cref="IdentityBuilder"/> for creating and configuring the identity system.</returns>
        public static IdentityBuilder AddIdentityCore<TUser, TRole>(this IServiceCollection services, Action<IdentityOptions> setupAction)
            where TUser : class
            where TRole : class
        {
            // Hosting doesn't add IHttpContextAccessor by default
            services.AddHttpContextAccessor();
            // Identity services
            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
            // No interface for the error describer so we can add errors without rev'ing the interface
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
            services.TryAddScoped<UserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>>();
            services.TryAddScoped<RoleManager<TRole>>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
        }
    }
}
