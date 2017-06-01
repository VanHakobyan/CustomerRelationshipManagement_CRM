using CRM.WebApi.Models;
using CRM.WebApp.Models;
using EntityLibrary;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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
        ModelFactory factory = new ModelFactory();
        public async Task<List<ContactResponseModel>> GetAllContacts()
        {

            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                List<Contact> dbContactList = await db.Contacts.ToListAsync();
                List<ContactResponseModel> responseContactList = new List<ContactResponseModel>();
                return dbContactList.Select(x => factory.CreateContactResponseModel(x)).ToList();
            }
            catch (EntitySqlException dbEx)
            {

                throw new EntitySqlException(dbEx.Message);
            }

        }
        public async Task<List<Contact>> GetContactPage(int start, int numberRows, bool flag)
        {
            var query = await db.Contacts.OrderBy(x => x.DateInserted).Skip(start).Take(numberRows).ToListAsync();

            for (int i = 0; i < query.Count; i++)
            {
                query[i].EmailLists = new List<EmailList>();
            }
            return query;
        }

        public async Task<ContactResponseModel> GetContactByGuid(Guid id)
        {
            var contact = await db.Contacts.FirstOrDefaultAsync(t => t.GuID == id);

            return factory.CreateContactResponseModel(contact);
        }

        public async Task<int> GetContactsPageCounter()
        {
            return await db.Contacts.CountAsync() > 10 ? await db.Contacts.CountAsync() / 10 : 1;
        }

        public async Task<List<ContactResponseModel>> GetContactsByGuIdList(List<Guid> GuIdList)
        {
            List<ContactResponseModel> ContactsList = new List<ContactResponseModel>();
            foreach (var guid in GuIdList)
            {
                ContactsList.Add(await GetContactByGuid(guid));
            }

            return ContactsList;
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


        public async Task<Contact> AddContact(ContactRequestModel contact)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Contact contacts = factory.CreateContact(contact);
                    db.Contacts.Add(contacts);
                    await db.SaveChangesAsync();
                    transaction.Commit();
                    return contacts;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<string> AddContactsFromFile(HttpRequestMessage request)
        {
            return await Task.Run(async () =>
            {
                string desctopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                Contact[] listOfContacts;

                var provider = new MultipartMemoryStreamProvider();
                await request.Content.ReadAsMultipartAsync(provider);

                var file = provider.Contents[0];
                var fileName = file.Headers.ContentDisposition.FileName;
                var filePath = desctopPath + '\\' + fileName;
                var buffer = await file.ReadAsByteArrayAsync();

                File.WriteAllBytes(filePath, buffer);

                var ExcelCSVFile = new ExcelQueryFactory(filePath);

                string fileExtension = fileName.Split('.').Last();


                if (!(fileExtension == "xlsx" || fileExtension == "csv"))
                {
                    return "FileNotFound";
                }

                HashSet<string> ContactColumnNames = new HashSet<string>(){
                       "FullName","CompanyName","Position","Country","Email"
                   };

                if (fileExtension == "csv")
                {
                    string[] CSVLines = File.ReadAllLines(filePath);
                    string[] columnNames = CSVLines[0].Split(';');

                    if (!ContactColumnNames.SetEquals(columnNames))
                    {
                        return "NotCorrectColumns";
                    }

                    int[] ColumnPositions = new int[columnNames.Length];
                    ColumnPositions[0] = Array.IndexOf(columnNames, "FullName");
                    ColumnPositions[1] = Array.IndexOf(columnNames, "CompanyName");
                    ColumnPositions[2] = Array.IndexOf(columnNames, "Position");
                    ColumnPositions[3] = Array.IndexOf(columnNames, "Country");
                    ColumnPositions[4] = Array.IndexOf(columnNames, "Email");


                    listOfContacts = new Contact[CSVLines.Length - 1];
                    string[] CellsOfRow;

                    for (int i = 1; i < CSVLines.Length; i++)
                    {
                        CellsOfRow = CSVLines[i].Split(';');

                        listOfContacts[i - 1] = new Contact
                        {
                            FullName = CellsOfRow[ColumnPositions[0]],
                            CompanyName = CellsOfRow[ColumnPositions[1]],
                            Position = CellsOfRow[ColumnPositions[2]],
                            Country = CellsOfRow[ColumnPositions[3]],
                            Email = CellsOfRow[ColumnPositions[4]]
                        };
                    }

                    //-------------------------------------------------------
                    File.Delete(filePath);
                    return await UploadHelper(listOfContacts);
                }

                string workSheetName = ExcelCSVFile.GetWorksheetNames().First();

                IEnumerable<string> columns = ExcelCSVFile.GetColumnNames(workSheetName);

                if (!ContactColumnNames.SetEquals(columns))
                {
                    return "NotCorrectColumns";
                }

                List<Row> rowsList = ExcelCSVFile.Worksheet(workSheetName).ToList();

                listOfContacts =
                    rowsList.Select(
                    cont =>
                    new Contact
                    {
                        FullName = cont["FullName"],
                        CompanyName = cont["CompanyName"],
                        Position = cont["Position"],
                        Country = cont["Country"],
                        Email = cont["Email"]
                    }).ToArray();

                //-------------------------------------------------------
                File.Delete(filePath);
                return await UploadHelper(listOfContacts);
            });
        }

        private async Task<string> UploadHelper(Contact[] listOfContacts)
        {
            foreach (var item in listOfContacts)
            {
                if (!RegexEmail(item.Email))
                {
                    return "InvalidEmail";
                }
            }

            foreach (var item in listOfContacts)
            {
                db.Contacts.Add(item);
            }
            await db.SaveChangesAsync();

            return "Ok";
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
            return await db.EmailLists.FirstOrDefaultAsync(t => t.EmailListID == id); //factory.CreateEmailResponseModel(email);
        }

        public async Task<EmailList> AddOrUpdateEmailList(EmailList еmailListForAddOrUpdate, EmailListRequestModel requestEmailListModel)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                еmailListForAddOrUpdate.EmailListName = requestEmailListModel.EmailListName;

                if (requestEmailListModel.Contacts != null)
                {
                    еmailListForAddOrUpdate.Contacts.Clear();
                    foreach (Guid guid in requestEmailListModel.Contacts)
                    {
                        var cont = await db.Contacts.FirstOrDefaultAsync(x => x.GuID == guid);
                        if (cont != null) еmailListForAddOrUpdate.Contacts.Add(cont);
                    }
                }

                db.EmailLists.AddOrUpdate(еmailListForAddOrUpdate);

                try
                {
                    await db.SaveChangesAsync();
                    transaction.Commit();
                    return еmailListForAddOrUpdate;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    if ((await EmailListExists(еmailListForAddOrUpdate.EmailListID)))
                    {
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }


        public async Task<EmailListResponseModel> RemoveEmailList(int id)
        {
            EmailList emailList = await db.EmailLists.FindAsync(id);
            db.EmailLists.Remove(emailList);
            await db.SaveChangesAsync();
            return factory.CreateEmailResponseModel(emailList);
        }

        public async Task<bool> EmailListExists(int id)
        {
            return await db.EmailLists.CountAsync(e => e.EmailListID == id) > 0;
        }
        public async Task SaveDb()
        {
            await db.SaveChangesAsync();
        }
        public bool RegexEmail(string email)
        {
            if (!Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                return false;
            return true;

        }
        public void Dispose()
        {
            db.Dispose();
        }
    }

}