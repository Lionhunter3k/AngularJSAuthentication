namespace AngularJSAuthentication.API.Migrations
{
    using AngularJSAuthentication.API.Entities;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public static class SeedExtensions
    {
        public static void Seed(this AuthContext context)
        {
            if (context.Clients.Count() > 0)
            {
                return;
            }
            context.Audiences.AddRange(BuildAudienceList());
            context.Clients.AddRange(BuildClientsList());
            context.SaveChanges();
        }

        private static List<Audience> BuildAudienceList()
        {
            return new List<Audience> { new Audience { Id = "099153c2625149bc8ecb3e85e03f0022",
                                                Base64Secret = "IxrAjDoa2FqElO7IhrSrUJELhUckePEPVpaePlS_Xaw",
                                                Name = "ResourceServer.Api 1" } };
        }

        private static List<Client> BuildClientsList()
        {
            List<Client> ClientsList = new List<Client>
            {
                new Client
                { Id = "ngAuthApp",
                    Secret= HashExtensions.GetHash("abc@123"),
                    Name="AngularJS front-end Application",
                    ApplicationType =  Models.ApplicationTypes.JavaScript,
                    Active = true,
                    RefreshTokenLifeTime = 7200,
                    AllowedOrigin = "http://ngauthenticationweb.azurewebsites.net"
                },
                new Client
                { Id = "consoleApp",
                    Secret=HashExtensions.GetHash("123@abc"),
                    Name="Console Application",
                    ApplicationType =Models.ApplicationTypes.NativeConfidential,
                    Active = true,
                    RefreshTokenLifeTime = 14400,
                    AllowedOrigin = "*"
                }
            };

            return ClientsList;
        }
    }
}
