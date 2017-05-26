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
    public class ContactsController : ApiController
    {
        private DataBaseCRMEntityes db = new DataBaseCRMEntityes();

        // GET: api/Contacts
        public async Task<List<ApiContactsModel>> GetContacts()
        {
            List<Contact> DbContactList = await db.Contacts.ToListAsync();
            List<ApiContactsModel> MyContactList = new List<ApiContactsModel>();

            foreach (var contact in DbContactList)
            {
                MyContactList.Add(new ApiContactsModel(contact));
            }

            return MyContactList;
        }

        // GET: api/Contacts/paje
        [ResponseType(typeof(Contact))]
        public async Task<IHttpActionResult> GetContact(int start, int numberRows, bool flag)
        {
            var query = await db.Contacts.OrderBy(x => x.DateInserted).Skip(start).Take(numberRows).ToListAsync();

            for (int i = 0; i < query.Count; i++)
            {
                query[i].EmailLists = new List<EmailList>();
            }

            return Ok(query);
        }

        // GET: api/Contacts/guid
        [ResponseType(typeof(ApiContactsModel))]
        public async Task<IHttpActionResult> GetContact(Guid id)
        {

            var contact = await db.Contacts.FirstOrDefaultAsync(t => t.GuID == id);
            if (contact == null)
            {
                return NotFound();
            }

            return Ok(new ApiContactsModel(contact));
        }


        //GET: api/Contacts/Npaje
        [Route("api/Contact/pages")]
        public async Task<int> GetContactsPageCount()
        {
            return await db.Contacts.CountAsync() > 10 ?await db.Contacts.CountAsync() / 10 : 1;
        }

        // PUT: api/Contacts/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutContact([FromBody]Contact contact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int id = contact.ContactId;
            Contact ContactsUpdate = await db.Contacts.FindAsync(id);

            if (ContactsUpdate == null)
            {
                return BadRequest();
            }

            ContactsUpdate.FullName = contact.FullName;
            ContactsUpdate.Country = contact.Country;
            ContactsUpdate.CompanyName = contact.CompanyName;
            ContactsUpdate.Email = contact.Email;
            ContactsUpdate.EmailLists = contact.EmailLists;

            db.Entry(ContactsUpdate).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            // TODO: swaped if and else
            catch (DbUpdateConcurrencyException)
            {
                if (await ContactExists(id))
                {
                    throw;
                }
                else
                {
                    return NotFound();
                }
            }

            return StatusCode(HttpStatusCode.NoContent);

        }

        // POST: api/Contacts
        [ResponseType(typeof(Contact))]
        public async Task<IHttpActionResult> PostContact(Contact contact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            contact.GuID = Guid.NewGuid();
            contact.DateInserted = DateTime.UtcNow;
            db.Contacts.Add(contact);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = contact.ContactId }, contact);
        }

        // DELETE: api/Contacts/5
        [ResponseType(typeof(Contact))]
        public async Task<IHttpActionResult> DeleteContact(int id)
        {
            Contact contact = await db.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            db.Contacts.Remove(contact);
            await db.SaveChangesAsync();

            return Ok(contact);
        }

        //// POST: api/Contacts
        //[Route("api/Contacts/upload")]
        //[ResponseType(typeof(Contact))]
        //public IHttpActionResult PostContactUpload([FromBody]byte[] array)
        //{
        //   // return CreatedAtRoute("DefaultApi", new { id = contact.ContactId }, contact);
        //}
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private async Task<bool> ContactExists(int id)
        {
            return  await db.Contacts.CountAsync(e => e.ContactId == id) > 0;
        }
    }
}