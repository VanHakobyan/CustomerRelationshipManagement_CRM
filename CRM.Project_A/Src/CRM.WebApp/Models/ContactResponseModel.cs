using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.WebApp.Models
{
    public class ContactResponseModel
    {
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public Guid? Guid { get; set; }
        public List<string> EmailLists { get; set; }
    }
}