using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using nH.Identity.Core;
using NHibernate;
using OAuthTutorial.Entities;
using OAuthTutorial.Models.AuthorizeViewModels;
using OAuthTutorial.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Controllers
{
    [Authorize]
    public class AuthorizeController : Controller
    {

        private readonly NHibernate.ISession _session;
        private readonly UserManager<User> _userManager;
        private readonly TicketCounter _ticketCounter;

        public AuthorizeController(NHibernate.ISession session, UserManager<User> userManager, TicketCounter ticketCounter)
        {
            _session = session;
            _userManager = userManager;
            _ticketCounter = ticketCounter;
        }


        public async Task<IActionResult> Index()
        {
            OpenIdConnectRequest request = HttpContext.GetOpenIdConnectRequest();
            OAuthClient client = await _session.GetAsync<OAuthClient>(request.ClientId);
            if (client == null)
            {
                return NotFound();
            }

            var vm = new AuthorizeViewModel
            {
                ClientId = client.ClientId,
                ClientDescription = client.ClientDescription,
                ClientName = client.ClientName,
                RedirectUri = request.RedirectUri,
                ResponseType = request.ResponseType,
                Scopes = String.IsNullOrWhiteSpace(request.Scope) ? new string[0] : request.Scope.Split(' '),
                State = request.State
            };
            return View(vm);
        }

        [HttpPost("deny")]
        public IActionResult Deny()
        {
            return LocalRedirect("/");
        }

        private async Task<bool> ValidateRequest(AuthorizeViewModel avm)
        {
            string clientId = avm.ClientId;
            OAuthClient client = await _session.GetAsync<OAuthClient>(clientId);
            if (client == null)
            {
                return false;
            }
            else
            {
                // Get the Scopes for this application from the query - disallow duplicates
                var scopes = new HashSet<OAuthScope>();
                if (avm.Scopes?.Length > 0)
                {
                    foreach (string s in avm.Scopes)
                    {
                        if (OAuthScope.NameInScopes(s))
                        {
                            OAuthScope scope = OAuthScope.GetScope(s);
                            scopes.Add(scope);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                avm.Scopes = scopes.Select(r => r.Name).ToArray();
                //Request.QueryString = QueryString.Create(new List<KeyValuePair<string, string>>
                //{
                //    new KeyValuePair<string, string>("grant_type", "code"),
                //    new KeyValuePair<string, string>("client_id", avm.ClientId),
                //    new KeyValuePair<string, string>("response_type", avm.ResponseType),
                //    new KeyValuePair<string, string>("scope", string.Join(" ", avm.Scopes)),
                //    new KeyValuePair<string, string>("redirect_uri", avm.RedirectUri),
                //    new KeyValuePair<string, string>("state", avm.State)
                //});
                return true;
            }
        }

        [HttpPost("accept")]
        public async Task<IActionResult> Accept([FromForm]AuthorizeViewModel avm)
        {
            User au = await _userManager.GetUserAsync(HttpContext.User);
            if (au == null || !await ValidateRequest(avm))
            {
                return LocalRedirect("/error");
            }
            AuthenticationTicket ticket = await _ticketCounter.MakeClaimsForInteractiveAsync(au, avm.ClientId, avm.State, avm.ResponseType, avm.Scopes, avm.RedirectUri);
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

    }
}
