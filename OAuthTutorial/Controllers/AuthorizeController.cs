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
    [Route("/authorize/")]
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

        [HttpPost("accept")]
        public async Task<IActionResult> Accept()
        {
            User au = await _userManager.GetUserAsync(HttpContext.User);
            if (au == null)
            {
                return LocalRedirect("/error");
            }
            OpenIdConnectRequest request = HttpContext.GetOpenIdConnectRequest();
            AuthorizeViewModel avm = await FillFromRequest(request);
            if (avm == null)
            {
                return LocalRedirect("/error");
            }
            AuthenticationTicket ticket = await _ticketCounter.MakeClaimsForInteractiveAsync(au, request.ClientId, request.State, request.ResponseType, avm.Scopes, request.RedirectUri);
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<AuthorizeViewModel> FillFromRequest(OpenIdConnectRequest OIDCRequest)
        {
            string clientId = OIDCRequest.ClientId;
            var client = await _session.GetAsync<OAuthClient>(clientId);
            if (client == null)
            {
                return null;
            }
            else
            {
                // Get the Scopes for this application from the query - disallow duplicates
                ICollection<OAuthScope> scopes = new HashSet<OAuthScope>();
                if (!String.IsNullOrWhiteSpace(OIDCRequest.Scope))
                {
                    foreach (string s in OIDCRequest.Scope.Split(' '))
                    {
                        if (OAuthScope.NameInScopes(s))
                        {
                            OAuthScope scope = OAuthScope.GetScope(s);
                            if (!scopes.Contains(scope))
                            {
                                scopes.Add(scope);
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                AuthorizeViewModel avm = new AuthorizeViewModel()
                {
                    ClientId = OIDCRequest.ClientId,
                    ResponseType = OIDCRequest.ResponseType,
                    State = OIDCRequest.State,
                    Scopes = String.IsNullOrWhiteSpace(OIDCRequest.Scope) ? new string[0] : OIDCRequest.Scope.Split(' '),
                    RedirectUri = OIDCRequest.RedirectUri
                };

                return avm;
            }
        }


    }
}
