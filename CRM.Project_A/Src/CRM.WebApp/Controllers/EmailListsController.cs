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

namespace CRM.WebApp.Controllers
{
    public class EmailListsController : ApiController
    {
        private DataBaseCRMEntityes db = new DataBaseCRMEntityes();

        // GET: api/EmailLists
        public List<EmailListModel> GetEmailLists()
        {
            List<EmailList> EntityContactList = db.EmailLists.ToList();
            List<EmailListModel> ModelContactList = new List<EmailListModel>();

            foreach (var contact in EntityContactList)
            {
                ModelContactList.Add(new EmailListModel(contact));
            }

            return ModelContactList;
        }

        // GET: api/EmailLists/5
        [ResponseType(typeof(EmailList))]
        public IHttpActionResult GetEmailList(int id)
        {
            var email = db.EmailLists.FirstOrDefault(t => t.EmailListID == id);
            if (email == null)
            {
                return NotFound();
            }

            return Ok(new EmailListModel(email));
        }

        // PUT: api/EmailLists/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutEmailList([FromBody] EmailListModel emailList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            EmailList EmailListUpdate = db.EmailLists.FirstOrDefault(t => t.EmailListID == emailList.EmailListID);
            if (EmailListUpdate == null)
            {
                return NotFound();
            }
            EmailListUpdate.EmailListName = emailList.EmailListName;
            ICollection<Contact> UpdatedContacts = new List<Contact>();
            foreach (string item in emailList.Contacts)
            {
                UpdatedContacts.Add(db.Contacts.FirstOrDefault(x => x.Email == item));
            }

            EmailListUpdate.Contacts.Clear();
            EmailListUpdate.Contacts = UpdatedContacts;
            // TODO:
            db.Entry(EmailListUpdate).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmailListExists(emailList.EmailListID))
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
        public IHttpActionResult PostEmailList([FromBody]EmailListModel emailList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var AddeddContacts = new List<Contact>();
            foreach (string listItem in emailList.Contacts)
            {
                AddeddContacts.Add(db.Contacts.FirstOrDefault(e => e.Email == listItem));
            }


            db.EmailLists.Add(new EmailList { EmailListName = emailList.EmailListName, Contacts = AddeddContacts });
            db.SaveChanges();

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
        public IHttpActionResult DeleteEmailList(int id)
        {
            EmailList emailList = db.EmailLists.Find(id);
            if (emailList == null)
            {
                return NotFound();
            }

            db.EmailLists.Remove(emailList);
            db.SaveChanges();

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

        private bool EmailListExists(int id)
        {
            return db.EmailLists.Count(e => e.EmailListID == id) > 0;
        }
    }
}