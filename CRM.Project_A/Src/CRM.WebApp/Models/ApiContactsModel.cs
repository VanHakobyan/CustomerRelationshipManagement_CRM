using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.WebApi.Models
{
    public class ApiContactsModel
    {
        public ApiContactsModel()
        {
            GuID = new Guid();
            DateInserted = null;
            EmailLists = new List<string>();
        }
        public ApiContactsModel(Contact contact)
        {
            EmailLists = new List<string>();
            FullName = contact.FullName;
            Position = contact.Position;
            Email = contact.Email;
            Country = contact.Country;
            CompanyName = contact.CompanyName;
            DateInserted = contact.DateInserted;
            GuID = contact.GuID;

            foreach (var item in contact.EmailLists)
            {
                EmailLists.Add(item.EmailListName);
            }
        }

        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public Guid GuID { get; set; }
        public DateTime? DateInserted { get; set; }
        public List<string> EmailLists { get; set; }
    }
}