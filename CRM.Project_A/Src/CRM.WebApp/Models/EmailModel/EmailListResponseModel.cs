using System.Collections.Generic;

namespace CRM.WebApp.Models
{
    public class EmailListResponseModel
    {
        public int EmailListId { get; set; }
        public string EmailListName { get; set; }
        public List<ContactResponseModel> Contacts { get; set; }
    }
}