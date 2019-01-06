using AngularJSAuthentication.API.Entities;
using AngularJSAuthentication.API.Migrations;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API
{
    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        public AuthContext()
            : base("AuthContext")
        {

        }

        static AuthContext()
        {
            Database.SetInitializer<AuthContext>(null);
            using (var authContext = new AuthContext())
            {
                authContext.Seed();
            }
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Audience> Audiences { get; set; }
    }

}