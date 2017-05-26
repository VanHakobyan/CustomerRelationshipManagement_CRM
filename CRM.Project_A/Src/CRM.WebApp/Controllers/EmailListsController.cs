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

namespace CRM.WebApp.Controllers
{
    public class EmailListsController : ApiController
    {
        private DataBaseCRMEntityes db = new DataBaseCRMEntityes();

        // GET: api/EmailLists
        public async Task<List<EmailListModel>> GetEmailLists()
        {
            List<EmailList> EntityContactList = await db.EmailLists.ToListAsync();
            List<EmailListModel> ModelContactList = new List<EmailListModel>();

            foreach (var contact in EntityContactList)
            {
                ModelContactList.Add(new EmailListModel(contact));
            }

            return ModelContactList;
        }

        // GET: api/EmailLists/5
        [ResponseType(typeof(EmailList))]
        public async Task<IHttpActionResult> GetEmailList(int id)
        {
            var email = await db.EmailLists.FirstOrDefaultAsync(t => t.EmailListID == id);
            if (email == null)
            {
                return NotFound();
            }

            return Ok(new EmailListModel(email));
        }

        // PUT: api/EmailLists/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEmailList([FromBody] EmailListModel emailList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            EmailList EmailListUpdate = await db.EmailLists.FirstOrDefaultAsync(t => t.EmailListID == emailList.EmailListID);
            if (EmailListUpdate == null)
            {
                return NotFound();
            }
            EmailListUpdate.EmailListName = emailList.EmailListName;
            ICollection<Contact> UpdatedContacts = new List<Contact>();
            foreach (string item in emailList.Contacts)
            {
                UpdatedContacts.Add(await db.Contacts.FirstOrDefaultAsync(x => x.Email == item));
            }

            EmailListUpdate.Contacts.Clear();
            EmailListUpdate.Contacts = UpdatedContacts;
            db.Entry(EmailListUpdate).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            // TODO: swaped if and else
            catch (DbUpdateConcurrencyException)
            {
                if (!(await EmailListExists(emailList.EmailListID)))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/EmailLists
        [ResponseType(typeof(EmailList))]
        public async Task<IHttpActionResult> PostEmailList([FromBody]EmailListModel emailList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var AddeddContacts = new List<Contact>();
            foreach (string listItem in emailList.Contacts)
            {
                AddeddContacts.Add(await db.Contacts.FirstOrDefaultAsync(e => e.Email == listItem));
            }


            db.EmailLists.Add(new EmailList { EmailListName = emailList.EmailListName, Contacts = AddeddContacts });
            await db.SaveChangesAsync();

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