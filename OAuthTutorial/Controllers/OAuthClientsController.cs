using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nH.Identity.Core;
using NHibernate;
using OAuthTutorial.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate.Linq;
using OAuthTutorial.Models.OAuthClientsViewModels;

namespace OAuthTutorial.Controllers
{
    [Authorize]
    public class OAuthClientsController : Controller
    {
        private readonly ISession _context;
        private readonly UserManager<User> _userManager;

        public OAuthClientsController(ISession context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: OAuthClients
        public async Task<IActionResult> Index()
        {
            string uid = _userManager.GetUserId(this.User);
            return View(await _context.Query<OAuthClient>().Where(x => x.Owner.Id == uid).Fetch(x => x.Owner).ToListAsync());
        }

        // POST: OAuthClients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientName,ClientDescription")] CreateClientViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var owner = await _userManager.GetUserAsync(this.User);
                var client = new OAuthClient()
                {
                    ClientDescription = vm.ClientDescription,
                    ClientName = vm.ClientName,
                    ClientId = Guid.NewGuid().ToString(),
                    ClientSecret = Guid.NewGuid().ToString(),
                    Owner = owner,
                };

                await _context.SaveAsync(client);
                await _context.FlushAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: OAuthClients/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            string uid = _userManager.GetUserId(this.User);
            var oAuthClient = await _context.Query<OAuthClient>().Fetch(x => x.Owner).FetchMany(x => x.RedirectURIs)
                .SingleOrDefaultAsync(m => m.ClientId == id && m.Owner.Id == uid);
            if (oAuthClient == null)
            {
                return NotFound();
            }

            EditClientViewModel vm = new EditClientViewModel()
            {
                ClientName = oAuthClient.ClientName,
                ClientDescription = oAuthClient.ClientDescription,
                ClientId = oAuthClient.ClientId,
                ClientSecret = oAuthClient.ClientSecret,
                RedirectUris = oAuthClient.RedirectURIs.ToArray()
            };

            return View(vm);
        }
    }
}
