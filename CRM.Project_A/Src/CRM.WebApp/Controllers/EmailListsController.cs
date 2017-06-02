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
using System.Threading.Tasks;
using CRM.WebApp.Infrastructure;
using CRM.WebApp.Models;
using System.Web;

namespace CRM.WebApp.Controllers
{
    [ExceptionCustomFilterAttribute]
    public class EmailListsController : ApiController
    {
        private ApplicationManager manager = new ApplicationManager();
        // GET: api/EmailLists
        public async Task<List<EmailListResponseModel>> GetEmailLists()
        {
            return await manager.GetAllEmailLis();
        }

        // GET: api/EmailLists/5
        [ResponseType(typeof(EmailListResponseModel))]
        public async Task<HttpResponseMessage> GetEmailList(int? id)
        {
            var email = await manager.GetEmailListById(id.Value);
            if (email == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            ModelFactory factory = new ModelFactory();
            return Request.CreateResponse(HttpStatusCode.OK, factory.CreateEmailResponseModel(email));

        }

        // PUT: api/EmailLists/5
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> PutEmailList( [FromUri] int id, [FromBody] EmailListRequestModel emailList)
        {
            if (!ModelState.IsValid)
                return Request.CreateResponse(HttpStatusCode.NotModified, ModelState);

            EmailList emailListToUpdate = await manager.GetEmailListById(id);
            EmailListResponseModel response = await manager.AddOrUpdateEmailList(emailListToUpdate, emailList);
            if (response == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK,response);
        }

        // POST: api/EmailLists
        [ResponseType(typeof(EmailList))]
        public async Task<HttpResponseMessage> PostEmailList([FromBody]EmailListRequestModel emailListRequest)
        {
            if (!ModelState.IsValid)
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);

            EmailList emailListSend = new EmailList();
            EmailListResponseModel emailList = await manager.AddOrUpdateEmailList(emailListSend, emailListRequest);
            return Request.CreateResponse(HttpStatusCode.Created, emailList);
        }

        // DELETE: api/EmailLists/5
        [ResponseType(typeof(EmailList))]
        public async Task<HttpResponseMessage> DeleteEmailList(int id)
        {
            var emailList = await manager.RemoveEmailList(id);
            if (emailList == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK,emailList);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                manager.Dispose();
            }
            base.Dispose(disposing);
        }

        private async Task<bool> EmailListExists(int id)
        {
            return await manager.EmailListExists(id);
        }
    }
}