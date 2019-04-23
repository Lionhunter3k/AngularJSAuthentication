﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASOS.Identity.Api.Models
{
    public class AuthorizeViewModel
    {
        [Display(Name = "Application")]
        public string ApplicationName { get; set; }
        [BindNever]
        public IDictionary<string, string> Parameters { get; set; }
        [Display(Name = "Scope")]
        public string Scope { get; set; }
    }
}