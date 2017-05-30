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


namespace CRM.WebApp.Controllers
{
    public class EmailListsController : ApiController
    {
        private DataBaseCRMEntityes db = new DataBaseCRMEntityes();
        private ApplicationManager manager = new ApplicationManager();
        // GET: api/EmailLists
        public async Task<List<EmailListResponseModel>> GetEmailLists()
        {
            return await manager.GetAllEmailLis();
        }

        // GET: api/EmailLists/5
        [ResponseType(typeof(EmailListResponseModel))]
        public async Task<IHttpActionResult> GetEmailList(int id)
        {
            var email = await manager.GetEmailListById(id);
            if (email == null)
            {
                return NotFound();
            }

            return Ok(email);
        }

        // PUT: api/EmailLists/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEmailList([FromBody] EmailListRequestModel emailList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            EmailList list = new EmailList();
            var res = await manager.AddOrUpdateEmailList(list, emailList);
            if (res==null)
            {
                return NotFound();
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/EmailLists
        [ResponseType(typeof(EmailList))]
        public async Task<IHttpActionResult> PostEmailList([FromBody]EmailListRequestModel emailListRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            EmailList emailListSend = new EmailList();
            EmailList emailList = await manager.AddOrUpdateEmailList(emailListSend, emailListRequest);
            //EmailListUpdate.EmailListName = emailList.EmailListName;
            //ICollection<Contact> UpdatedContacts = new List<Contact>();
            //foreach (string item in emailList.Contacts)
            //{
            //    UpdatedContacts.Add(db.Contacts.FirstOrDefault(x => x.Email == item));
            //}

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/EmailLists/5
        [ResponseType(typeof(EmailList))]
        public async Task<IHttpActionResult> DeleteEmailList(int id)
        {
            EmailList emailList = await db.EmailLists.FindAsync(id);
            if (emailList == null)
            {
                return NotFound();
            }

            db.EmailLists.Remove(emailList);
            await db.SaveChangesAsync();

            return Ok(emailList);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private async Task<bool> EmailListExists(int id)
        {
            return await db.EmailLists.CountAsync(e => e.EmailListID == id) > 0;
        }
    }
}