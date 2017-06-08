using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRM.WebApp.Models
{
    public class ContactRequestModel
    {
        [Required(ErrorMessage = "Full name is required"), StringLength(100, MinimumLength = 1, ErrorMessage = "The full name must be specified.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Company is required"), StringLength(100, MinimumLength = 1, ErrorMessage = "The Compzny name must be specified.")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Position is required"), StringLength(100, MinimumLength = 1, ErrorMessage = "The Position must be specified.")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Country is required"), StringLength(100, MinimumLength = 1, ErrorMessage = "The full name must be specified.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Country is required"), EmailAddress(ErrorMessage = "The Email must be specified")]
        public string Email { get; set; }
        
    }
}