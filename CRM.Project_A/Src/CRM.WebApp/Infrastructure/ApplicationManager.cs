using CRM.WebApp.Models;
using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.WebApp.Infrastructure
{
    public class ApplicationManager : IDisposable
    {


        private readonly DataBaseCRMEntityes db = new DataBaseCRMEntityes();
        private readonly ModelFactory factory = new ModelFactory();

        #region Contacts
        public async Task<List<ContactResponseModel>> GetAllContacts()
        {
            try
            {
                //db.Configuration.LazyLoadingEnabled = false;
                List<Contact> dbContactList = await db.Contacts.ToListAsync();
                return dbContactList.Select(x => factory.CreateContactResponseModel(x)).ToList();
            }
            catch (EntitySqlException dbEx)
            {
                throw new EntitySqlException(dbEx.Message);
            }
        }

        public async Task<List<Contact>> GetContactPage(int start, int numberRows, bool flag)
        {
            try
            {
                var query = await db.Contacts.OrderBy(x => x.DateInserted).Skip(start).Take(numberRows).ToListAsync();
                for (int i = 0; i < query.Count; i++)
                {
                    query[i].EmailLists = new List<EmailList>();
                }
                return query;

            }
            catch
            {
                throw;
            }
        }

        public async Task<ContactResponseModel> GetContactByGuid(Guid id)
        {
            try
            {
                var contact = await db.Contacts.FirstOrDefaultAsync(t => t.GuID == id);
                return factory.CreateContactResponseModel(contact);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetContactsPageCounter()
        {
            try
            {
                return await db.Contacts.CountAsync() > 10 ? await db.Contacts.CountAsync() / 10 : 1;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ContactResponseModel>> GetContactsByGuIdList(List<Guid> GuIdList)
        {
            List<ContactResponseModel> ContactsList = new List<ContactResponseModel>();
            try
            {
                foreach (var guid in GuIdList)
                {
                    ContactsList.Add(await GetContactByGuid(guid));
                }
                return ContactsList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateContact(Guid guid, ContactRequestModel contact)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                Contact dbContactToUpdate;
                try
                {
                    dbContactToUpdate = await db.Contacts.FirstOrDefaultAsync(c => c.GuID == guid);
                }
                catch (Exception)
                {
                    throw;
                }
                if (dbContactToUpdate == null) return false;
                dbContactToUpdate.FullName = contact.FullName;
                dbContactToUpdate.Country = contact.Country;
                dbContactToUpdate.Position = contact.Position;
                dbContactToUpdate.CompanyName = contact.CompanyName;
                dbContactToUpdate.Email = contact.Email;

                db.Entry(dbContactToUpdate).State = EntityState.Modified;
                try
                {
                    await db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ContactExistsAsync(guid))
                    {
                        return false;
                    }
                    else
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                return true;
            }
        }


        public async Task<ContactResponseModel> AddContact(ContactRequestModel contact)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var contacts = factory.CreateContact(contact);
                    db.Contacts.Add(contacts);
                    await db.SaveChangesAsync();
                    transaction.Commit();
                    var response = factory.CreateContactResponseModel(contacts);
                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        public async Task<ContactResponseModel> RemoveContact(Guid guid)
        {
            try
            {
                var contact = await db.Contacts.FirstOrDefaultAsync(c => c.GuID == guid);
                var resModel = factory.CreateContactResponseModel(contact);
                db.Contacts.Remove(contact);
                await db.SaveChangesAsync();
                return resModel;

            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> RemoveContactByGuidList(List<Guid> guidlist)
        {
            foreach (var item in guidlist)
            {
                await RemoveContact(item);
            }
            return true;
        }

        public async Task<bool> ContactExistsAsync(Guid id)
        {
            try
            {
                return await db.Contacts.CountAsync(e => e.GuID == id) > 0;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region EmailLists
        public async Task<List<EmailListResponseModel>> GetAllEmailLis()
        {
            try
            {
                List<EmailList> entityContactList = await db.EmailLists.ToListAsync();
                List<EmailListResponseModel> ModelContactList = new List<EmailListResponseModel>();
                return entityContactList.Select(f => factory.CreateEmailResponseModel(f)).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<EmailList> GetEmailListById(int id)
        {
            try
            {
                return await db.EmailLists.FirstOrDefaultAsync(t => t.EmailListID == id);
            }
            catch
            {
                throw;
            }
        }

        public async Task<EmailListResponseModel> AddEmailList(EmailList еmailListForAdd, EmailListRequestModel requestEmailListModel)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                еmailListForAdd.EmailListName = requestEmailListModel.EmailListName;

                if (requestEmailListModel.Contacts != null)
                {
                    еmailListForAdd.Contacts.Clear();
                    foreach (Guid guid in requestEmailListModel.Contacts)
                    {
                        var contacts = await db.Contacts.FirstOrDefaultAsync(x => x.GuID == guid);
                        if (contacts != null) еmailListForAdd.Contacts.Add(contacts);
                    }
                }
                try
                {
                    db.EmailLists.AddOrUpdate(еmailListForAdd);
                    await db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    if ((await EmailListExists(еmailListForAdd.EmailListID)))
                        return null;
                    else
                        throw;
                }
                return factory.CreateEmailResponseModel(еmailListForAdd);
            }
        }

        public async Task<EmailListResponseModel> AddAtEmailList(EmailList еmailListForAddOrUpdate, List<Guid> guidList)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                //еmailListForAddOrUpdate.EmailListName = requestEmailListModel.EmailListName;
                if (guidList.Count != 0)
                {
                    foreach (Guid guid in guidList)
                    {
                        var contacts = await db.Contacts.FirstOrDefaultAsync(x => x.GuID == guid);
                        if (contacts != null)
                            еmailListForAddOrUpdate.Contacts.Add(contacts);
                    }
                }
                try
                {
                    await db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    if ((await EmailListExists(еmailListForAddOrUpdate.EmailListID)))
                        return null;
                    else
                        throw;
                }
                return factory.CreateEmailResponseModel(еmailListForAddOrUpdate);
            }
        }
        public async Task<EmailListResponseModel> RemoveAtEmailList(EmailList еmailListForAddOrUpdate, List<Guid> guidList)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                //еmailListForAddOrUpdate.EmailListName = guidList.EmailListName;
                if (guidList.Count != 0)
                {
                    foreach (Guid guid in guidList)
                    {
                        var contacts = await db.Contacts.FirstOrDefaultAsync(x => x.GuID == guid);
                        if (contacts != null)
                            еmailListForAddOrUpdate.Contacts.Remove(contacts);
                    }
                }
                try
                {
                    await db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    if ((await EmailListExists(еmailListForAddOrUpdate.EmailListID)))
                        return null;
                    else
                        throw;
                }

                return factory.CreateEmailResponseModel(еmailListForAddOrUpdate);
            }
        }

        public async Task<EmailListResponseModel> RemoveEmailList(int id)
        {
            try
            {
                EmailList emailList = await db.EmailLists.FindAsync(id);
                db.EmailLists.Remove(emailList);
                await db.SaveChangesAsync();
                return factory.CreateEmailResponseModel(emailList);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> EmailListExists(int id)
        {
            try
            {
                return await db.EmailLists.CountAsync(e => e.EmailListID == id) > 0;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Templates
        public async Task<List<TemplateResponseModel>> GetTemplates()
        {
            try
            {
                var templateList = await db.Templates.ToListAsync();
                var response = new List<TemplateResponseModel>();
                return templateList.Select(x => factory.CreateTemplateResponseModel(x)).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> TemplateExistsAsync(int id)
        {
            return await db.Templates.CountAsync(e => e.TemplateId == id) > 0;
        }
        #endregion

        #region Filtering
        public async Task<List<ContactResponseModel>> GetFilteredContacts(ContactFilterModel contactFilterData, string[] orderParams)
        {
            return await Task.Run(() =>
            {
                List<ContactResponseModel> result = new List<ContactResponseModel>();
                string resultQuery = "SELECT * FROM Contacts";
                List<string> conditions = new List<string>();

                //if (!string.IsNullOrEmpty(contactFilterData.FullName))
                //    conditions.Add($" FullName LIKE '%{contactFilterData.FullName}%'");

                //if (!string.IsNullOrEmpty(contactFilterData.CompanyName))
                //    conditions.Add($" CompanyName LIKE '%{contactFilterData.CompanyName}%'");

                //if (!string.IsNullOrEmpty(contactFilterData.Position))
                //    conditions.Add($" Position LIKE '%{contactFilterData.Position}%'");

                //if (!string.IsNullOrEmpty(contactFilterData.Country))
                //    conditions.Add($" Country LIKE '%{contactFilterData.Country}%'");

                //if (!string.IsNullOrEmpty(contactFilterData.Email))
                //    conditions.Add($" Email LIKE '%{contactFilterData.Email}%'");

                if (conditions.Count != 0)
                {
                    resultQuery += " WHERE";
                    foreach (var item in conditions)
                    {
                        resultQuery += item + " AND";
                    }
                    resultQuery = resultQuery.Substring(0, resultQuery.Length - 4);
                }

                if (orderParams != null && orderParams.Length != 0)
                {
                    resultQuery += " ORDER BY ";
                    foreach (var item in orderParams)
                    {
                        resultQuery += $"{item.Replace('_', ' ')},";
                    }
                    resultQuery = resultQuery.TrimEnd(',');
                }

                List<Contact> contactList;
                try
                {
                    contactList = db.Database.SqlQuery<Contact>(resultQuery).ToList();
                }
                catch (Exception)
                {
                    return null;
                }

                return contactList.Select(contact => factory.CreateContactResponseModel(contact)).ToList();
            });
        }


        public async Task<List<EmailListResponseModel>> GetFilteredEmailLists(string emailListName, string param)
        {
            return await Task.Run(() =>
            {
                List<EmailListResponseModel> result = new List<EmailListResponseModel>();
                string resultQuery = "SELECT * FROM EmailLists";

                if (!string.IsNullOrEmpty(emailListName))
                    resultQuery += $" WHERE EmailListName LIKE '%{emailListName}%'";

                if (param != null && param.Length != 0)
                {
                    resultQuery += " ORDER BY ";
                    resultQuery += $"{param.Replace('_', ' ')},";
                    resultQuery = resultQuery.TrimEnd(',');
                }

                List<EmailList> EmailLists;
                try
                {
                    EmailLists = db.Database.SqlQuery<EmailList>(resultQuery).ToList();
                }
                catch (Exception)
                {
                    return null;
                }

                return EmailLists.Select(emailList => factory.CreateEmailResponseModel(emailList)).ToList();
            });
        }

        #endregion

        #region Reset
        public async Task<bool> Reset()
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var allcontacts = await db.Contacts.DefaultIfEmpty().ToListAsync();
                    var allEmailLists = await db.EmailLists.DefaultIfEmpty().ToListAsync();

                    if (allcontacts[0] != null)
                        db.Contacts.RemoveRange(allcontacts);

                    if (allEmailLists[0] != null)
                        db.EmailLists.RemoveRange(allEmailLists);

                    List<EmailList> TeamA = new List<EmailList>() { new EmailList() { EmailListName = "Team A" } };
                    List<EmailList> BetConstract = new List<EmailList>() { new EmailList() { EmailListName = "BetConstruct" } };
                    List<EmailList> OtherPeople = new List<EmailList>() { new EmailList() { EmailListName = "Other team members" } };

                    List<Contact> startContacts = new List<Contact>()
                    {
                        new Contact() { FullName = "Vanik Hakobyan",CompanyName = "YSU",Country = "Armenia",Position = "Student", Email = "vanhakobyan1996@gmail.com",  EmailLists = TeamA,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Khachatur Sukiasyan",CompanyName = "Microsoft",Country = "Armenia",Position = "Freelancer", Email = "khachatur124@gmail.com", EmailLists = TeamA,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Tsovinar Ghazaryan",CompanyName = "VTB Bank",Country = "Armenia",Position = "Credit Controller", Email = "tsovinar.ghazaryan@yahoo.com",EmailLists = TeamA,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Tatevik Begjanyan",CompanyName = "TB LLC",Country = "Armenia",Position = "Foreign Affairs Manager", Email = "tkbegjanyan@gmail.com", EmailLists = TeamA,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Narine Boyakhchyan",CompanyName = "NB LLC",Country = "Armenia",Position = "Director", Email = "narine.boyakhchyan@gmail.com", EmailLists = TeamA,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Zara Muradyan",CompanyName = "ZM LLC",Country = "Armenia",Position = "Director", Email = "zara.muradyann@gmail.com", EmailLists = TeamA,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },

                        new Contact() { FullName = "Tigran Vardanyan",CompanyName = "Candle",Country = "Switzerland",Position = "Theoretician", Email = "tigran_vardanyan@yahoo.com", EmailLists = OtherPeople,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Aram Jamkotchian",CompanyName = "MIC",Country = "Spain",Position = "Lead", Email = "aram532@yandex.ru", EmailLists = OtherPeople,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Lusine Hovsepyan",CompanyName = "SCDM GmbH",Country = "Armenia",Position = "Financial Analyst", Email = "lusine@hovsepyan.am", EmailLists = OtherPeople,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Gayane Khachatryan",CompanyName = "Khachatryan LLC",Country = "Armenia",Position = "Owner", Email = "gayane.jane@gmail.com", EmailLists = OtherPeople,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Narek Yegoryan",CompanyName = "NPUA",Country = "Armenia",Position = "Student", Email = "yegoryan.narek@gmail.com", EmailLists = OtherPeople,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Lusine Khachatryan",CompanyName = "LK LLC",Country = "Armenia",Position = "Director", Email = "luskhachatryann@gmail.com", EmailLists = OtherPeople,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },

                        new Contact() { FullName = "George Voyatzis",CompanyName = "BetConstruct",Country = "London - United Kingdom",Position = "Commercial Director", Email = "g.voyatzis@betconstruct.com", EmailLists = BetConstract,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Anna Poghosyan",CompanyName = "BetConstruct",Country = "Yerevan - Armenia",Position = "International Development Director", Email = "a.poghosyan@betconstruct.com", EmailLists = BetConstract,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Anna Shahbazyan",CompanyName = "BetConstruct",Country = "Montevideo - Uruguay",Position = "Regional Director", Email = "a.shahbazyan@betconstruct.com", EmailLists = BetConstract,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Samvel Nersisyan",CompanyName = "BetConstruct",Country = "Yerevan - Armenia",Position = "Head of Business Development", Email = "samvel.nersisyan@betconstruct.com", EmailLists = BetConstract,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Zorair Asadour",CompanyName = "BetConstruct",Country = "Cape Town - South Africa",Position = "Regional Director", Email = "z.asadour@betconstruct.com", EmailLists = BetConstract,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Stephan Mamikonjan",CompanyName = "BetConstruct",Country = "Vienna - Austria",Position = "Regional Director", Email = "stephan.mamikonjan@betconstruct.com", EmailLists = BetConstract,GuID = Guid.NewGuid(),DateInserted = DateTime.Now }
                     };

                    db.Contacts.AddRange(startContacts);
                    db.EmailLists.AddRange(BetConstract);
                    db.EmailLists.AddRange(OtherPeople);
                   
                    await db.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
        #endregion

        public void Dispose()
        {
            db.Dispose();
        }
    }
}