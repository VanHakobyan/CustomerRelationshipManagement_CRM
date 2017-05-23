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
    public class ContactsController : ApiController
    {

        // GET: api/Contacts
        public List<Contact> GetContacts()
        {
            using (DataBaseCRMEntities db = new DataBaseCRMEntities())
            {
                return db.Contacts.ToListAsync().Result;
            }

        }

        // GET: api/Contacts/5
        [ResponseType(typeof(Contact))]
        public IHttpActionResult GetContact(int id)
        {
            using (DataBaseCRMEntities db = new DataBaseCRMEntities())
            {
                Contact contact = db.Contacts.Find(id);
                if (contact == null)
                {
                    return NotFound();
                }

                return Ok(contact);
            }
        }

        // PUT: api/Contacts/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutContact([FromUri]int id, [FromBody]Contact contact)
        {
            using (DataBaseCRMEntities db = new DataBaseCRMEntities())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != contact.ContactId)
                {
                    return BadRequest();
                }

                db.Entry(contact).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(id))
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
        }

        // POST: api/Contacts
        [ResponseType(typeof(Contact))]
        public IHttpActionResult PostContact([FromBody]Contact contact)
        {
            using (DataBaseCRMEntities db = new DataBaseCRMEntities())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                contact.DateInserted = DateTime.UtcNow;
                contact.GuID = Guid.NewGuid();
                db.Contacts.Add(contact);
                db.SaveChanges();

                return CreatedAtRoute("DefaultApi", new { id = contact.ContactId }, contact);
            }
        }
        // DELETE: api/Contacts/5
        [ResponseType(typeof(Contact))]
        public IHttpActionResult DeleteContact(int id)
        {
            using (DataBaseCRMEntities db = new DataBaseCRMEntities())
            {
                Contact contact = db.Contacts.Find(id);
                if (contact == null)
                {
                    return NotFound();
                }

                db.Contacts.Remove(contact);
                db.SaveChanges();

                return Ok(contact);
            }
        }

        protected override void Dispose(bool disposing)
        {
            using (DataBaseCRMEntities db = new DataBaseCRMEntities())
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        private bool ContactExists(int id)
        {
            using (DataBaseCRMEntities db = new DataBaseCRMEntities())
            {
                return db.Contacts.Count(e => e.ContactId == id) > 0;
            }
        }
    }
}