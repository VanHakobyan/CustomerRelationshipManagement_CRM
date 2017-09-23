using CRM.WebApp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CRM.WebApp.Controllers
{
    [ExceptionCustomFilter]
    //[Authorize]
    public class EmailSenderController : ApiController
    {
        private readonly EmailProvider provider = new EmailProvider();
        private readonly ApplicationManager manager = new ApplicationManager();
        [Route("api/EmailSender/{templateId}")]
        public async Task<IHttpActionResult> PostSendEmails([FromBody] List<Guid> guIdList, [FromUri] int templateId)
        {
            var contactsForSending = await manager.GetContactsByGuIdList(guIdList);
            if (contactsForSending == null)
            {
                return NotFound();
            }
            try
            {
                provider.SendEmailList(contactsForSending, templateId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
            return Ok("Thank you!!! ");
        }
        [Route("api/EmailSender/{EmailListId}/{templateId}")]
        public async Task<HttpResponseMessage> PostSendEmailList(int emailListId, int templateId)
        {
            if (!await manager.TemplateExistsAsync(templateId))
                return Request.CreateResponse(HttpStatusCode.NotFound);
            if (!await provider.SendMailToMailingList(emailListId, templateId))
                return Request.CreateResponse(HttpStatusCode.Conflict);
            return Request.CreateResponse(HttpStatusCode.OK);
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
