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


        // GET: OAuthClients/Create
        public IActionResult Create()
        {
            return View();
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

        // POST: OAuthClients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ClientDescription", "RedirectUris")] EditClientViewModel vm)
        {
            string uid = _userManager.GetUserId(this.User);
            OAuthClient client = await _context.Query<OAuthClient>().Where(x => x.ClientId == id && x.Owner.Id == uid).Fetch(x => x.Owner).FetchMany(x => x.RedirectURIs).FirstOrDefaultAsync();
            if (client == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var originalUris = client.RedirectURIs;
                foreach (string s in vm.RedirectUris)
                {
                    if (String.IsNullOrWhiteSpace(s))
                    {
                        continue;
                    }
                    var fromOld = originalUris.FirstOrDefault(x => x == s);
                    if (fromOld == null)
                    {
                        // this 's' is new.
                        var rdi = s;
                        originalUris.Add(rdi);
                    }
                }

                // Marking deleted Redirect URIs for Deletion.
                originalUris.Except(vm.RedirectUris).ToList().Select(x => originalUris.Remove(x));

                client.ClientDescription = vm.ClientDescription;
                await _context.UpdateAsync(client);
                await _context.FlushAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // POST: OAuthClients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {

            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            string uid = _userManager.GetUserId(this.User);
            var oAuthClient = await _context.Query<OAuthClient>()
                .SingleOrDefaultAsync(m => m.ClientId == id && m.Owner.Id == uid);

            if (oAuthClient == null)
            {
                return NotFound();
            }

            await _context.DeleteAsync(oAuthClient);
            await _context.FlushAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: OAuthClients/ResetSecret/
        [HttpPost, ActionName("ResetSecret")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetClientSecret(string id)
        {

            string uid = _userManager.GetUserId(this.User);
            OAuthClient client = await _context.Query<OAuthClient>().Where(x => x.ClientId == id && x.Owner.Id == uid).Fetch(x => x.Owner).FirstOrDefaultAsync();
            if (client == null)
            {
                return NotFound();
            }

            client.ClientSecret = Guid.NewGuid().ToString();
            await _context.UpdateAsync(client);
            await _context.FlushAsync();
            return RedirectToAction(id, "OAuthClients/Edit");
        }
    }
}
