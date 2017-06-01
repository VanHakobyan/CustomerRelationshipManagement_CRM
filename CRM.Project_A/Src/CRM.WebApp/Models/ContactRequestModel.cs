using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRM.WebApp.Models
{
    public class ContactRequestModel
    {
        [Required]
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string Country { get; set; }
        [RegularExpression("...")]
        public string Email { get; set; }
        //public Guid? Guid { get; set; }
        //public DateTime? DateInserted { get; set; }
        //public List<string> MailingLists { get; set; }
    }
}