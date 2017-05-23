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

namespace CRM.WebApp.Controllers
{
    public class EmailListsController : ApiController
    {
        private DataBaseCRMEntities db = new DataBaseCRMEntities();

        // GET: api/EmailLists
        public IQueryable<EmailList> GetEmailLists()
        {
            return db.EmailLists;
        }

        // GET: api/EmailLists/5
        [ResponseType(typeof(EmailList))]
        public IHttpActionResult GetEmailList(int id)
        {
            EmailList emailList = db.EmailLists.Find(id);
            if (emailList == null)
            {
                return NotFound();
            }

            return Ok(emailList);
        }

        // PUT: api/EmailLists/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutEmailList(int id, EmailList emailList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != emailList.EmailListID)
            {
                return BadRequest();
            }

            db.Entry(emailList).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmailListExists(id))
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
        public IHttpActionResult PostEmailList(EmailList emailList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.EmailLists.Add(emailList);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = emailList.EmailListID }, emailList);
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