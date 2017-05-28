using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EntityLibrary;
namespace CRM.WebApp.Models
{
    public class MailingListModel
    {
        public MailingListModel() { }

        public MailingListModel(EmailList emailList)
        {
            MailingListName = emailList.EmailListName;
            Contacts = emailList.Contacts.Select(x => x.Email).ToList();
        }
        public int EmailListId { get; set; }
        public string MailingListName { get; set; }
        public List<string> Contacts { get; set; }
    }
}