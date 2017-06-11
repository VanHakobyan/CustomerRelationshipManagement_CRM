using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EntityLibrary;
using System.Threading.Tasks;
using CRM.WebApp.Infrastructure;
using CRM.WebApp.Models;

namespace CRM.WebApp.Controllers
{
    [ExceptionCustomFilter]
    //[Authorize]
    public class EmailListsController : ApiController
    {
        private ApplicationManager manager = new ApplicationManager();
        // GET: api/EmailLists
        public async Task<HttpResponseMessage> GetEmailLists()
        {
            var emailList = await manager.GetAllEmailLis();
            if (emailList == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            return Request.CreateResponse(HttpStatusCode.OK, emailList);
        }

        // GET: api/EmailLists/5
        [Route("api/EmailLists/{id}")]
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

        [ Route("api/EmailLists/add/{id}")]
        public async Task<HttpResponseMessage> PutEmailListAdd([FromUri] int id, [FromBody] List<Guid> guidList)
        {
            if (!ModelState.IsValid)
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            EmailList emailListToUpdate = await manager.GetEmailListById(id);
            if (emailListToUpdate == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            EmailListResponseModel emailListRes = await manager.AddAtEmailList(emailListToUpdate, guidList);
            if (emailListRes == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            return Request.CreateResponse(HttpStatusCode.OK, emailListRes);
        }
        [Route("api/EmailLists/remove/{id}")]
        public async Task<HttpResponseMessage> PutEmailListRemove([FromUri] int id, [FromBody] List<Guid> guidList)
        {
            if (!ModelState.IsValid)
                return Request.CreateResponse(HttpStatusCode.NotModified, ModelState);
            EmailList emailListToUpdate = await manager.GetEmailListById(id);
            if (emailListToUpdate == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            EmailListResponseModel emailListRes = await manager.RemoveAtEmailList(emailListToUpdate, guidList);
            return Request.CreateResponse(HttpStatusCode.OK, emailListRes);
        }
        // POST: api/EmailLists
        public async Task<HttpResponseMessage> PostEmailList([FromBody]EmailListRequestModel emailListRequest)
        {
            if (!ModelState.IsValid)
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);

            EmailList emailListSend = new EmailList();
            EmailListResponseModel emailList = await manager.AddEmailList(emailListSend, emailListRequest);
            return Request.CreateResponse(HttpStatusCode.Created, emailList);
        }

        // DELETE: api/EmailLists/5
        [Route("api/EmailLists/delete/{id}")]
        public async Task<HttpResponseMessage> DeleteEmailList(int id)
        {
            var emailList = await manager.RemoveEmailList(id);
            if (emailList == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.OK, emailList);
        }

        [Route("api/EmailLists/filter")]
        public async Task<HttpResponseMessage> PostContactsFilter([FromUri]string emailListName, [FromUri] string param)
        {
            List<EmailListResponseModel> response = await manager.GetFilteredEmailLists(emailListName, param);
            if (response == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "wrong Url, can't work with database");

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

        private async Task<bool> EmailListExists(int id)
        {
            return await manager.EmailListExists(id);
        }
    }
}