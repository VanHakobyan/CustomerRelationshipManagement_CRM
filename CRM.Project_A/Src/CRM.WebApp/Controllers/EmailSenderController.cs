using CRM.WebApp.Infrastructure;
using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CRM.WebApp.Controllers
{
    public class EmailSenderController : ApiController
    {
        EmailProvider provider = new EmailProvider();
        ApplicationManager manager = new ApplicationManager();
        public async Task<IHttpActionResult> PostSendEmails([FromBody] List<Guid> GuIdList, [FromUri] int TemplateId)
        {
            List<Contact> ContactsForSending = await manager.GetContactsByGuIdList(GuIdList);
            if (ContactsForSending == null)
            {
                return NotFound();
            }
            try
            {
                provider.SendEmail(ContactsForSending, TemplateId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                
            }
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
