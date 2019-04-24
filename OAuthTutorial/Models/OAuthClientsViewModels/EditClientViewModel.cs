using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Models.OAuthClientsViewModels
{
    public class EditClientViewModel
    {
        [Required]
        [MinLength(1)]
        [MaxLength(500)]
        public string ClientDescription { get; set; }

        public string ClientName { get; internal set; }

        public string ClientId { get; internal set; }

        public string ClientSecret { get; internal set; }

        public string[] RedirectUris { get; set; } = new string[0];
    }
}
