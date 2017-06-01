using System.Collections.Generic;

namespace CRM.WebApp.Models
{
    public class EmailListResponseModel
    {
        public int EmailListID { get; set; }
        public string EmailListName { get; set; }
        public List<string> Contacts { get; set; }
    }
}