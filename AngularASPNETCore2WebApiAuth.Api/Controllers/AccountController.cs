using AngularASPNETCore2WebApiAuth.Api.Extensions;
using AngularASPNETCore2WebApiAuth.Api.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nH.Identity.Core;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AngularASPNETCore2WebApiAuth.Api.Controllers
{
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly NHibernate.ISession _session;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public AccountController(ISession session, IMapper mapper, UserManager<User> userManager)
        {
            _session = session;
            _mapper = mapper;
            _userManager = userManager;
        }

        // POST api/accounts
        [HttpPost]
        [Route("register")] // /api/account/hello
        public async Task<IActionResult> Register([FromBody]RegistrationViewModel model)
        {
            using (var tx = _session.BeginTransaction())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdentity = _mapper.Map<User>(model);

                var result = await _userManager.CreateAsync(userIdentity, model.Password);

                if (!result.Succeeded)
                    return new BadRequestObjectResult(ModelState.AddErrorsToModelState(result));
                await tx.CommitAsync();
                return new OkObjectResult("Account created");
            }
        }
    }
}
