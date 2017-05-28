using CRM.WebApp.Models;
using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.WebApp.Infrastructure
{
    public class ModelFactory
    {
        private  DataBaseCRMEntityes db = new DataBaseCRMEntityes();

        public  ContactResponseModel CreateContactResponseModel(ContactRequestModel crmRequest)
        {
            return new ContactResponseModel
            {
                FullName = crmRequest.FullName,
                CompanyName = crmRequest.CompanyName,
                Position = crmRequest.Position,
                Country = crmRequest.Country,
                Email = crmRequest.Email,
                Guid = Guid.NewGuid(),
                DateInserted = DateTime.UtcNow,
                EmailLists = new List<string>()
            };
        }
        public  ContactResponseModel CreateContactResponseModel(Contact contacts)
        {
            return new ContactResponseModel
            {
                FullName = contacts.FullName,
                CompanyName = contacts.CompanyName,
                Position = contacts.Position,
                Country = contacts.Country,
                Email = contacts.Email,
                Guid = contacts.GuID,
                DateInserted = contacts.DateInserted,
                EmailLists = contacts.EmailLists.Select(e => e.EmailListName).ToList()
            };
        }
        public  Contact CreateContact(ContactRequestModel crmRequest)
        {
            var contacts = new Contact
            {
                FullName = crmRequest.FullName,
                CompanyName = crmRequest.CompanyName,
                Position = crmRequest.Position,
                Country = crmRequest.Country,
                Email = crmRequest.Email,
                GuID = Guid.NewGuid(),
                DateInserted = DateTime.Now,
            };
            var list = new List<EmailList>();
            using (db)
            {
                foreach (var emailListName in crmRequest.MailingLists)
                {
                    list.AddRange(db.EmailLists.Where(emailList => emailList.EmailListName == emailListName));
                }
            }
            contacts.EmailLists = list;
            return contacts;
        }

    }
}