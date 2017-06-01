using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using EntityLibrary;
using CRM.WebApi.Models;
using System.Threading.Tasks;
using CRM.WebApp.Infrastructure;
using CRM.WebApp.Models;
using System.Text.RegularExpressions;

namespace CRM.WebApp.Controllers
{
    [ExceptionCustomFilterAttribute]
    public class ContactsController : ApiController
    {
        //private DataBaseCRMEntityes db = new DataBaseCRMEntityes();
        private ApplicationManager manager = new ApplicationManager();

        // GET: api/Contacts
        public async Task<HttpResponseMessage> GetContacts()
        {
            return Request.CreateResponse(HttpStatusCode.OK, await manager.GetAllContacts());
        }

        // GET: api/Contacts/paje
        [ResponseType(typeof(Contact))]
        public async Task<IHttpActionResult> GetContact(int start, int numberRows, bool flag)
        {
            var contact = await manager.GetContactPage(start, numberRows, flag);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        // GET: api/Contacts/guid
        [ResponseType(typeof(Contact))]
        public async Task<IHttpActionResult> GetContactGuid(Guid id)
        {
            var contact = await manager.GetContactByGuid(id);
            if (contact == null)
            {
                return NotFound();
            }

            return Ok(contact);
        }


        //GET: api/Contacts/Npaje
        [Route("api/Contact/pages")]
        public async Task<int> GetContactsPageCount()
        {
            return await manager.GetContactsPageCounter();
        }

        //PUT: api/Contacts/5
        [ResponseType(typeof(void))]
        [HttpPut]
        public async Task<IHttpActionResult> PutContact(Guid guid, [FromBody] ContactRequestModel contact)
        {
            if (!manager.RegexEmail(contact.Email))
                return BadRequest("Email address is not valid");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (await manager.UpdateContact(guid, contact))
                return StatusCode(HttpStatusCode.NoContent);

            return NotFound();

        }
        // POST: api/Contacts
        [ResponseType(typeof(ContactRequestModel))]
        public async Task<HttpResponseMessage> PostContact(ContactRequestModel contact)
        {
            if (!manager.RegexEmail(contact.Email))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Email address is not valid");

            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }


            Contact createdContacts = await manager.AddContact(contact);

            return Request.CreateResponse(HttpStatusCode.Created, createdContacts);
        }

        // DELETE: api/Contacts/5
        [ResponseType(typeof(ContactResponseModel))]
        public async Task<IHttpActionResult> DeleteContact([FromBody]List<Guid> guid)
        {

            if (!await manager.RemoveContactByGuidList(guid))
            {
                return NotFound();
            }

            return Ok();
        }

        [Route("api/Contacts/upload")]
        [ResponseType(typeof(Contact))]
        public async Task<IHttpActionResult> PostContactUpload()
        {
            string response;
            try
            {
                response = await manager.AddContactsFromFile(Request);
            }
            catch (Exception)
            {
                return BadRequest();
            }
            if (response == "FileNotFound")
                return BadRequest("Wrong File format");
            if (response == "NotCorrectColumns")
                return BadRequest("Wrong columns of excel/csv sheet");
            if (response == "InvalidEmail")
                return BadRequest("Email address is not valid");
            if (response == "Ok")
                return StatusCode(HttpStatusCode.OK);
            return StatusCode(HttpStatusCode.NotFound);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                manager.Dispose();
            }
            base.Dispose(disposing);
        }

        private async Task<bool> ContactExists(Guid id)
        {
            return await manager.ContactExistsAsync(id);
        }
    }
}