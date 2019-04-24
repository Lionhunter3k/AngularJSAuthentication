using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Services
{
    public class OAuthScope
    {

        private static readonly List<OAuthScope> _AllScopes = new List<OAuthScope>();
        public static IReadOnlyList<OAuthScope> AllScopes => _AllScopes;

        public readonly string Name;
        public readonly string Description;

        private OAuthScope(string name, string description)
        {
            if (NameInScopes(name))
            {
                throw new InvalidOperationException($"Tried to add an OAuthScope with the name {name}, but this name already existed");
            }

            this.Name = name;
            this.Description = description;
            _AllScopes.Add(this);
        }

        public static bool NameInScopes(string name)
        {
            return _AllScopes.Any(x => x.Name == name);
        }

        public static OAuthScope GetScope(string name)
        {
            return _AllScopes.First(x => x.Name == name);
        }

        public readonly static OAuthScope UserReadEmail = new OAuthScope("user-read-email", "Permission to know your email address");
        public readonly static OAuthScope UserReadBirthdate = new OAuthScope("user-read-birthdate", "Permission to know your birthdate");
        public readonly static OAuthScope UserModifyEmail = new OAuthScope("user-modify-email", "Permission to change your email address");
        public readonly static OAuthScope UserModifyBirthdate = new OAuthScope("user-modify-birthdate", "Permission to change your birthdate");

    }
}
