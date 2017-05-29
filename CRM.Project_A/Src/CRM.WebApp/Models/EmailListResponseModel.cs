using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.WebApp.Models
{
    public class EmailListResponseModel
    {
        public int EmailListID { get; set; }
        public string EmailListName { get; set; }
        public List<string> Contacts { get; set; }
    }
}