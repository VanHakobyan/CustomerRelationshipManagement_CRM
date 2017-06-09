using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.WebApp.Models
{
    public class EmailListRequestModel
    {
      
        [Required(ErrorMessage = "Email List Name is required"), StringLength(100, MinimumLength = 1, ErrorMessage = "The full name must be specified.")]
        public string EmailListName { get; set; }
        public List<Guid> Contacts { get; set; }
    }
}