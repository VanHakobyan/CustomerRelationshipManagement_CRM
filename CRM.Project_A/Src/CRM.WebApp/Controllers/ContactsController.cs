using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CRM.WebApp.Models;
using System.Threading.Tasks;
using CRM.WebApp.Infrastructure;
using System.Net.Http.Headers;
using System.Web;
using System.IO;

namespace CRM.WebApp.Controllers
{
    [ExceptionCustomFilterAttribute]
    //[Authorize]
    public class ContactsController : ApiController
    {
        private ApplicationManager manager = new ApplicationManager();
        private ParsingProvider provider = new ParsingProvider();
        private LoggerManager logger = new LoggerManager();

        // GET: api/Contacts
        public async Task<HttpResponseMessage> GetContacts()
        {
            List<ContactResponseModel> contacts = await manager.GetAllContacts();
            if (contacts == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, contacts);
        }

        // GET: api/Contacts/paje
        public async Task<HttpResponseMessage> GetContact(int start, int numberRows, bool flag)
        {
            var contact = await manager.GetContactPage(start, numberRows, flag);
            if (contact == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, contact);
        }

        // GET: api/Contacts/guid
        [Route("api/Contacts/{id}")]
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
        [HttpPut, Route("api/contacts/{guid}")]
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
        public async Task<HttpResponseMessage> PostContact([FromBody]ContactRequestModel contact)
        {
            //if (!manager.RegexEmail(contact.Email))
            //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Email address is not valid");

            if (!ModelState.IsValid || ReferenceEquals(contact, null))
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);

            ContactResponseModel createdContacts = await manager.AddContact(contact);
            return Request.CreateResponse(HttpStatusCode.Created, createdContacts);
        }

        // DELETE: api/Contacts
        public async Task<HttpResponseMessage> DeleteContact([FromBody]List<Guid> guid)
        {
            if (!await manager.RemoveContactByGuidList(guid))
                return Request.CreateResponse(HttpStatusCode.NotFound);
            if (guid.Count == 0)
                return Request.CreateResponse(HttpStatusCode.NotImplemented);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("api/Contacts/upload")]
        public async Task<HttpResponseMessage> PostContactUpload()
        {
            List<ContactResponseModel> response;
            response = await provider.AddContactsFromFile(Request);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        [Route("api/Contacts/filter")]
        public async Task<HttpResponseMessage> PostContactsFilter([FromBody]ContactFilterModel contactFilterData, [FromUri] string[] param)
        {
            List<ContactResponseModel> response = await manager.GetFilteredContacts(contactFilterData, param);
            if (response == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "wrong Url, can't work with database");

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
        [Route("api/Contacts/reset"), HttpGet]
        public async Task<HttpResponseMessage> GetReset()
        {
            var response = new HttpResponseMessage();
            string htmlPath = HttpContext.Current.Server.MapPath($"~//Templates//reset.html");
            string responseText = File.ReadAllText(htmlPath).Replace("{date}", DateTime.Now.ToString());

            response.Content = new StringContent(responseText);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            if (await manager.Reset())
                return response;
            response.StatusCode = HttpStatusCode.InternalServerError;
            return response;
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