using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CRM.WebApp.Infrastructure;
using EntityLibrary;
using System.Net.Mail;
using System.Web.Http.Description;

namespace CRM.WebApp.Controllers
{
    public class EmailSenderController : ApiController
    {
        ApplicationManager manager = new ApplicationManager();
       // EmailProvider emailProvider = new EmailProvider();
        //[ResponseType(typeof(Contact))]
        //[HttpPost]
        public async Task<IHttpActionResult> PostSendEmailsPost([FromBody] List<Guid> GuIdList, [FromUri] int TamplateId)
        {
            List<Contact> ContactsToSend = await manager.GetContactsByGuIdList(GuIdList);
            if (ReferenceEquals(ContactsToSend, null)) return NotFound();

            EmailProvider.SendEmail(ContactsToSend, TamplateId);
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                manager.Dispose();
            }
            base.Dispose(disposing);
        }
       
    }
}
