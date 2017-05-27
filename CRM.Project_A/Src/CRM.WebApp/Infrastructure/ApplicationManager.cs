using CRM.WebApi.Models;
using CRM.WebApp.Models;
using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;

namespace CRM.WebApp.Infrastructure
{
    public class ApplicationManager : IDisposable
    {

        // public ApplicationManager(){}
        DataBaseCRMEntityes db = new DataBaseCRMEntityes();

        public async Task<List<ApiContactsModel>> GetAllContacts()
        {
            db.Configuration.LazyLoadingEnabled = false;
            List<Contact> DbContactList = await db.Contacts.ToListAsync();
            List<ApiContactsModel> MyContactList = new List<ApiContactsModel>();
            foreach (var contact in DbContactList)
            {
                MyContactList.Add(new ApiContactsModel(contact));
            }
            return MyContactList;
        }
        public async Task<List<Contact>> GetContactPaje(int start, int numberRows, bool flag)
        {
            var query = await db.Contacts.OrderBy(x => x.DateInserted).Skip(start).Take(numberRows).ToListAsync();

            for (int i = 0; i < query.Count; i++)
            {
                query[i].EmailLists = new List<EmailList>();
            }
            return query;
        }

        public async Task<Contact> GetContactByGuid(Guid id)
        {

            var contact = await db.Contacts.FirstOrDefaultAsync(t => t.GuID == id);
            return contact;
        }

        public async Task<int> GetContactsPageCounter()
        {
            return await db.Contacts.CountAsync() > 10 ? await db.Contacts.CountAsync() / 10 : 1;
        }

        public async Task<List<Contact>> GetContactsByGuIdList(List<Guid> GuIdList)
        {
            List<Contact> ContactsList = new List<Contact>();
            foreach (var guid in GuIdList)
            {
                ContactsList.Add(await GetContactByGuid(guid));
            }

            return ContactsList;
        }
        public async Task<bool> UpdateContact(ViewContact contact)
        {
            Contact dbContactToUpdate = await GetContactByGuid(contact.GuID);

            if (dbContactToUpdate == null) return false;

            dbContactToUpdate.FullName = contact.FullName;
            dbContactToUpdate.Country = contact.Country;
            dbContactToUpdate.CompanyName = contact.CompanyName;
            dbContactToUpdate.Email = contact.Email;

            db.Entry(dbContactToUpdate).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {

                if (!await ContactExistsAsync(contact.GuID))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }


        public async Task<Contact> AddContact(Contact contact)
        {

            contact.GuID = Guid.NewGuid();
            contact.DateInserted = DateTime.UtcNow;
            db.Contacts.Add(contact);
            await db.SaveChangesAsync();

            return contact;
        }
        public async Task<Contact> RemoveContact(int id)
        {
            Contact contact = await db.Contacts.FindAsync(id);


            db.Contacts.Remove(contact);
            await db.SaveChangesAsync();

            return contact;
        }
        public async Task<bool> ContactExistsAsync(Guid id)
        {
            return await db.Contacts.CountAsync(e => e.GuID == id) > 0;
        }
        public async void SaveDb()
        {
            await db.SaveChangesAsync();
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }

}