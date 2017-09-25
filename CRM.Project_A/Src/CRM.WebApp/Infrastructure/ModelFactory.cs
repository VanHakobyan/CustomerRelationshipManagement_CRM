using CRM.WebApp.Models;
using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.WebApp.Infrastructure
{
    public class ModelFactory
    {
        private DataBaseCRMEntityes db = new DataBaseCRMEntityes();

        public ContactResponseModel CreateContactResponseModel(ContactRequestModel crmRequest)
        {
            return new ContactResponseModel
            {
                FullName = crmRequest.FullName,
                CompanyName = crmRequest.CompanyName,
                Position = crmRequest.Position,
                Country = crmRequest.Country,
                Email = crmRequest.Email,
                Guid = Guid.NewGuid(),
                EmailLists = new List<string>()
            };
        }

        public ContactResponseModel CreateContactResponseModel(Contact contacts)
        {
            return new ContactResponseModel
            {
                FullName = contacts.FullName,
                CompanyName = contacts.CompanyName,
                Position = contacts.Position,
                Country = contacts.Country,
                Email = contacts.Email,
                Guid = contacts.GuID,
                EmailLists = contacts.EmailLists.Select(e => e.EmailListName).ToList()
            };
        }

        public Contact CreateContact(ContactRequestModel crmRequest)
        {
            Contact contacts = new Contact
            {
                FullName = crmRequest.FullName,
                CompanyName = crmRequest.CompanyName,
                Position = crmRequest.Position,
                Country = crmRequest.Country,
                Email = crmRequest.Email,
                GuID = Guid.NewGuid(),
                DateInserted = DateTime.Now,
                DateModified = DateTime.Now,
                EmailLists = new List<EmailList>()
            };
            return contacts;
        }

        public EmailList CreateEmailRequestModel(EmailListRequestModel requestModel)
        {
            EmailList emailList = new EmailList()
            {
                Contacts = new List<Contact>(),
                EmailListName = requestModel.EmailListName
            };
            return emailList;
        }

        public EmailListResponseModel CreateEmailResponseModel(EmailList emailList)
        {
            return new EmailListResponseModel() {
                EmailListId = emailList.EmailListID,
                EmailListName = emailList.EmailListName,
                Contacts = emailList.Contacts.Select(CreateContactResponseModel).ToList()
                };
        }
        public TemplateResponseModel CreateTemplateResponseModel(Template template)
        {
            return new TemplateResponseModel
            {
                Id = template.TemplateId,
                TemplateName = template.TemplateName
            };
        }

    }
}