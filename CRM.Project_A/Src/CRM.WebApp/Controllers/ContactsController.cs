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
using CRM.WebApp.Models;
using System.Threading.Tasks;
using CRM.WebApp.Infrastructure;

using System.Text.RegularExpressions;

namespace CRM.WebApp.Controllers
{
    [ExceptionCustomFilterAttribute]
    public class ContactsController : ApiController
    {
        private ApplicationManager manager = new ApplicationManager();
        private LoggerManager logger = new LoggerManager();
        // GET: api/Contacts
        public async Task<HttpResponseMessage> GetContacts()
        {
            return Request.CreateResponse(HttpStatusCode.OK, await manager.GetAllContacts());
        }

        // GET: api/Contacts/paje
        [ResponseType(typeof(Contact))]
        public async Task<HttpResponseMessage> GetContact(int start, int numberRows, bool flag)
        {
            var contact = await manager.GetContactPage(start, numberRows, flag);
            if (contact == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, contact);
        }

        // GET: api/Contacts/guid
        [ResponseType(typeof(Contact)),Route("api/Contacts/{id}")]
        public async Task<HttpResponseMessage> GetContactGuid([FromUri]Guid id)
        {
            var contact = await manager.GetContactByGuid(id);
            if (contact == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, contact);
        }


        //GET: api/Contacts/Npaje
        [Route("api/Contact/pages")]
        public async Task<int> GetContactsPageCount()
        {
            return await manager.GetContactsPageCounter();
        }

        //PUT: api/Contacts/5
        [ResponseType(typeof(void))]
        [HttpPut,Route("api/contacts/{guid}")]
        public async Task<HttpResponseMessage> PutContact(Guid guid, [FromBody] ContactRequestModel contact)
        {
            //if (!manager.RegexEmail(contact.Email))
            //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Email address is not valid");
            if (!ModelState.IsValid || ReferenceEquals(contact, null))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }
            if (await manager.UpdateContact(guid, contact))
                return Request.CreateResponse(HttpStatusCode.OK, contact);

            return Request.CreateResponse(HttpStatusCode.NotFound);

        }
        // POST: api/Contacts
        [ResponseType(typeof(ContactRequestModel))]
        public async Task<HttpResponseMessage> PostContact([FromBody]ContactRequestModel contact)
        {
            //if (!manager.RegexEmail(contact.Email))
            //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Email address is not valid");

            if (!ModelState.IsValid || ReferenceEquals(contact, null))
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);

            ContactResponseModel createdContacts = await manager.AddContact(contact);
            return Request.CreateResponse(HttpStatusCode.Created, createdContacts);
        }

        // DELETE: api/Contacts/5
        [ResponseType(typeof(ContactResponseModel))]
        public async Task<HttpResponseMessage> DeleteContact([FromBody]List<Guid> guid)
        {
            if (!await manager.RemoveContactByGuidList(guid) )
                return Request.CreateResponse(HttpStatusCode.NotFound);
            if (guid.Count == 0)
                return Request.CreateResponse(HttpStatusCode.NotImplemented);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [ResponseType(typeof(ContactRequestModel)), Route("api/Contacts/upload")]
        public async Task<HttpResponseMessage> PostContactUpload()
        {
            List<ContactResponseModel> response;
            response = await manager.AddContactsFromFile(Request);
            return Request.CreateResponse(HttpStatusCode.OK, response);
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