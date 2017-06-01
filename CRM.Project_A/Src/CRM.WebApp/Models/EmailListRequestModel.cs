using System;
using System.Collections.Generic;

namespace CRM.WebApp.Models
{
    public class EmailListRequestModel
    {
        //public int EmailListId { get; set; }
        public string EmailListName { get; set; }
        public List<Guid> Contacts { get; set; }
    }
}