using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.WebApp.Models
{
    public class EmailListModel
    {
        public EmailListModel(EmailList emaillist)
        {
            EmailListID = emaillist.EmailListID;
            EmailListName = emaillist.EmailListName;

            Contacts = new List<string>();

            foreach (var emails in emaillist.Contacts)
            {
                Contacts.Add(emails.Email);
            }
        }
        public EmailListModel(){}
        public int EmailListID { get; set; }
        public string EmailListName { get; set; }
        public virtual List<string> Contacts { get; set; }
    }
}