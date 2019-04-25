using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nH.Identity.Core;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Controllers
{
    [Route("/api/v1/")]
    [Authorize(AuthenticationSchemes = AspNet.Security.OAuth.Validation.OAuthValidationDefaults.AuthenticationScheme)]
    public class APIController : Controller
    {

        private readonly ILogger _logger;
        private readonly ISession _context;
        private readonly UserManager<User> _userManager;

        public APIController(ILogger<ManageController> logger, ISession context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // Unauthenticated Methods - available to the public
        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok("Hello");
        }

        // Authenticated Methods - only available to those with a valid Access Token
        // Unscoped Methods - Authenticated methods that do not require any specific Scope
        [HttpGet("clientcount")]
        public IActionResult ClientCount()
        {
            return Ok("Client Count Get Request was successful but this endpoint is not yet implemented");
        }

        // Scoped Methods - Authenticated methods that require certain scopes
        [HttpGet("birthdate")]
        [Authorize(AuthenticationSchemes = AspNet.Security.OAuth.Validation.OAuthValidationDefaults.AuthenticationScheme, Policy = "user-read-birthdate")]
        public IActionResult GetBirthdate()
        {
            return Ok("Birthdate Get Request was successful but this endpoint is not yet implemented");
        }

        [HttpGet("email")]
        [Authorize(AuthenticationSchemes = AspNet.Security.OAuth.Validation.OAuthValidationDefaults.AuthenticationScheme, Policy = "user-read-email")]
        public IActionResult GetEmail()
        {
            return Ok("Email Get Request was successful but this endpoint is not yet implemented");
        }

        [HttpPut("birthdate")]
        [Authorize(AuthenticationSchemes = AspNet.Security.OAuth.Validation.OAuthValidationDefaults.AuthenticationScheme, Policy = "user-modify-birthdate")]
        public IActionResult ChangeBirthdate(string birthdate)
        {
            return Ok("Birthdate Put successful but this endpoint is not yet implemented");
        }

        [HttpPut("email")]
        [Authorize(AuthenticationSchemes = AspNet.Security.OAuth.Validation.OAuthValidationDefaults.AuthenticationScheme, Policy = "user-modify-email")]
        public IActionResult ChangeEmail(string email)
        {
            return Ok("Email Put request received, but function is not yet implemented");
        }

        // Dynamic Scope Methods - Authenticated methods that return additional information the more scopes are supplied
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok("User Profile Get request received, but function is not yet implemented");
        }
    }
}
