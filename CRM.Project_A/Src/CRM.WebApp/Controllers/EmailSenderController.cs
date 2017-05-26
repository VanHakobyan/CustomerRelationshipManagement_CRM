using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CRM.WebApp.Infrastructure;
using EntityLibrary;
namespace CRM.WebApp.Controllers
{
    public class EmailSenderController : ApiController
    {

        ApplicationManager manager = new ApplicationManager();
        //public async Task<IHttpActionResult> PostSendEmails([FromBody] List<Guid> GuIdList, [FromUri] int TamplateId)
        //{
        //    List<Contact> ContactsToSend = await manager.GetContactList(GuIdList);
        //    if (ReferenceEquals(ContactsToSend, null)) return NotFound();

        //    await manager.SendContactsEmail(ContactsToSend, TamplateId);
        //    return Ok();

        //}

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
