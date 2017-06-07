using CRM.WebApp.Models;
using EntityLibrary;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations;

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
            return await db.EmailLists.FirstOrDefaultAsync(t => t.EmailListID == id); 
        }

        public async Task<EmailListResponseModel> AddEmailList(EmailList еmailListForAddOrUpdate, EmailListRequestModel requestEmailListModel)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                еmailListForAddOrUpdate.EmailListName = requestEmailListModel.EmailListName;

                if (requestEmailListModel.Contacts != null)
                {
                    еmailListForAddOrUpdate.Contacts.Clear();
                    foreach (Guid guid in requestEmailListModel.Contacts)
                    {
                        var contacts = await db.Contacts.FirstOrDefaultAsync(x => x.GuID == guid);
                        if (contacts != null) еmailListForAddOrUpdate.Contacts.Add(contacts);
                    }
                }
                try
                {
                    db.EmailLists.AddOrUpdate(еmailListForAddOrUpdate);
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
            EmailList emailList = await db.EmailLists.FindAsync(id);
            db.EmailLists.Remove(emailList);
            await db.SaveChangesAsync();
            return factory.CreateEmailResponseModel(emailList);
        }

        public async Task<bool> EmailListExists(int id)
        {
            return await db.EmailLists.CountAsync(e => e.EmailListID == id) > 0;
        }
        #endregion

        #region Templates
        public async Task<List<TemplateResponseModel>> GetTemplates()
        {
            var templateList = await db.Templates.ToListAsync();
            var response = new List<TemplateResponseModel>();
            return templateList.Select(x => factory.CreateTemplateResponseModel(x)).ToList();
        }

        public async Task<bool> TemplateExistsAsync(int id)
        {
            return await db.Templates.CountAsync(e => e.TemplateId == id) > 0;
        }
        #endregion

        #region uploading
        public async Task<List<ContactResponseModel>> AddContactsFromFile(HttpRequestMessage request)
        {
            string filePath = null;
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    string tempPath = System.Web.HttpContext.Current?.Request.MapPath("~//Templates");

                    List<ContactResponseModel> response = new List<ContactResponseModel>();
                    List<Contact> listOfContacts = null;

                    var provider = new MultipartMemoryStreamProvider();
                    await request.Content.ReadAsMultipartAsync(provider);

                    var file = provider.Contents[0];
                    var fileName = file.Headers.ContentDisposition.FileName;
                    var correctedFileName = "new" + fileName.Split('"', '\\').First(s => !string.IsNullOrEmpty(s));
                    filePath = tempPath + '\\' + correctedFileName;
                    string fileExtension = correctedFileName.Split('.').Last();
                    var buffer = await file.ReadAsByteArrayAsync();

                    File.WriteAllBytes(filePath, buffer);

                    if (fileExtension == "xlsx")
                    {
                        listOfContacts = ReadExcelFileDOM(filePath);
                    }
                    else
                        if (fileExtension == "csv")
                    {
                        listOfContacts = ReadCSVFile(filePath);
                    }
                    else
                    {
                        throw new FileNotFoundException("Wrong extension of file");
                    }

                    foreach (var item in listOfContacts)
                    {
                        item.GuID = Guid.NewGuid();
                        item.DateInserted = DateTime.Now;
                        item.DateModified = DateTime.Now;
                        item.EmailLists = new List<EmailList>();

                        db.Contacts.Add(item);
                        response.Add(factory.CreateContactResponseModel(item));
                    }
                    await db.SaveChangesAsync();
                    transaction.Commit();
                    return response;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    File.Delete(filePath);
                }

            }
        }

        private static List<Contact> ReadExcelFileDOM(string filename)
        {

            string[] strRowValues = new string[5];
            List<Contact> result = new List<Contact>();
            Contact contact;

            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(filename, true))
                {
                    WorkbookPart workbookPart = document.WorkbookPart;
                    IEnumerable<Sheet> Sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                    string relationshipId = Sheets?.First().Id.Value;
                    WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(relationshipId);
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    int i = 1;
                    int j = 0;
                    string value;

                    int[] valueIndexes = new int[5];

                    foreach (Row r in sheetData.Elements<Row>())
                    {

                        foreach (Cell c in r.Elements<Cell>())
                        {
                            if (c == null) continue;
                            value = c.InnerText;
                            if (c.DataType != null)
                            {
                                var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                                if (stringTable != null)
                                {
                                    value = stringTable.SharedStringTable.
                                        ElementAt(int.Parse(value)).InnerText;
                                }
                            }
                            strRowValues[j] = value;
                            j = j + 1;
                        }

                        if (i == 1)
                        {
                            valueIndexes[0] = Array.IndexOf(strRowValues, "FullName");
                            valueIndexes[1] = Array.IndexOf(strRowValues, "CompanyName");
                            valueIndexes[2] = Array.IndexOf(strRowValues, "Position");
                            valueIndexes[3] = Array.IndexOf(strRowValues, "Country");
                            valueIndexes[4] = Array.IndexOf(strRowValues, "Email");

                            if (valueIndexes.Contains(-1))
                            {
                                throw new FileNotFoundException("Wrong columns in Excel");
                            }
                            j = 0;
                            i = i + 1;
                            continue;
                        }

                        j = 0;
                        i = i + 1;
                        if (strRowValues.Any(p => p == null)) continue;
                        contact = new Contact();
                        contact.FullName = strRowValues[valueIndexes[0]];
                        contact.CompanyName = strRowValues[valueIndexes[1]];
                        contact.Position = strRowValues[valueIndexes[2]];
                        contact.Country = strRowValues[valueIndexes[3]];
                        contact.Email = strRowValues[valueIndexes[4]];

                        CheckContact(contact);
                        result.Add(contact);
                    }
                    return result;
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        static List<Contact> ReadCSVFile(string filePath)
        {
            try
            {
                string[] CSVLines = File.ReadAllLines(filePath);
                string[] columnNames = CSVLines[0].Split(';', ',');

                int[] ColumnPositions = new int[columnNames.Length];
                ColumnPositions[0] = Array.IndexOf(columnNames, "FullName");
                ColumnPositions[1] = Array.IndexOf(columnNames, "CompanyName");
                ColumnPositions[2] = Array.IndexOf(columnNames, "Position");
                ColumnPositions[3] = Array.IndexOf(columnNames, "Country");
                ColumnPositions[4] = Array.IndexOf(columnNames, "Email");

                if (ColumnPositions.Contains(-1))
                {
                    throw new FileNotFoundException("Wrong column names in CSV");
                }

                List<Contact> listOfContacts = new List<Contact>();
                string[] CellsOfRow;
                Contact tempContact;

                for (int i = 1; i < CSVLines.Length; i++)
                {
                    CellsOfRow = CSVLines[i].Split(';', ',');
                    tempContact = new Contact
                    {
                        FullName = CellsOfRow[ColumnPositions[0]],
                        CompanyName = CellsOfRow[ColumnPositions[1]],
                        Position = CellsOfRow[ColumnPositions[2]],
                        Country = CellsOfRow[ColumnPositions[3]],
                        Email = CellsOfRow[ColumnPositions[4]]
                    };
                    CheckContact(tempContact);

                    listOfContacts.Add(tempContact);
                }
                return listOfContacts;
            }
            catch (Exception)
            {
                throw;
            }

        }

        static void CheckContact(Contact contact)
        {
            if (string.IsNullOrEmpty(contact.FullName) || contact.FullName.Length > 100)
            {
                throw new FileNotFoundException("Wrong data of FullName");
            }
            if (string.IsNullOrEmpty(contact.CompanyName) || contact.CompanyName.Length > 100)
            {
                throw new FileNotFoundException("Wrong data of CompanyName");
            }
            if (string.IsNullOrEmpty(contact.Position) || contact.Position.Length > 100)
            {
                throw new FileNotFoundException("Wrong data of Position");
            }
            if (string.IsNullOrEmpty(contact.Country) || contact.Country.Length > 100)
            {
                throw new FileNotFoundException("Wrong data of Position");
            }

            var emailAddress = new EmailAddressAttribute();
            if (!emailAddress.IsValid(contact.Email))
            {
                throw new FileNotFoundException("Wrong data of Email");
            }
        }

        #endregion


        #region filtering
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

        #region reset
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
                    
                    List<EmailList> startEmailList = new List<EmailList>() { new EmailList() { EmailListName = "StartEmailList" } };
                    List<Contact> startContacts = new List<Contact>()
                    {
                        new Contact() { FullName = "Tsovinar Ghazaryan",CompanyName = "VTB Bank",Country = "Armenia",Position = "Credit Controller", Email = "tsovinar.ghazaryan@yahoo.com",EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Vanik Hakobyan",CompanyName = "YSU",Country = "Armenia",Position = "Student", Email = "vanhakobyan1996@gmail.com",  EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Khachatur Sukiasyan",CompanyName = "Microsoft",Country = "Armenia",Position = "Freelancer", Email = "khachatur124@gmail.com", EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Tigran Vardanyan",CompanyName = "Candle",Country = "Switzerland",Position = "Theoretician", Email = "tigran_vardanyan@yahoo.com", EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Aram Jamkotchian",CompanyName = "MIC",Country = "Spain",Position = "Lead", Email = "aram532@yandex.ru", EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Lusine Hovsepyan",CompanyName = "SCDM GmbH",Country = "Armenia",Position = "Financial Analyst", Email = "lusine@hovsepyan.am", EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Gayane Khachatryan",CompanyName = "Khachatryan LLC",Country = "Armenia",Position = "Owner", Email = "gayane.jane@gmail.com", EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Aghasi Lorsabyan",CompanyName = "TUMO",Country = "Armenia",Position = "Developer", Email = "lorsabyan@gmail.com", EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Narek Yegoryan",CompanyName = "NPUA",Country = "Armenia",Position = "Student", Email = "yegoryan.narek@gmail.com", EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Narine Boyakhchyan",CompanyName = "NB LLC",Country = "Armenia",Position = "Director", Email = "narine.boyakhchyan@gmail.com", EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now },
                        new Contact() { FullName = "Tatevik Begjanyan",CompanyName = "LLC",Country = "Armenia",Position = "Foreign Affairs Manager", Email = "tkbegjanyan@gmail.com", EmailLists = startEmailList,GuID = Guid.NewGuid(),DateInserted = DateTime.Now }
                     };

                    db.Contacts.AddRange(startContacts);
                    db.EmailLists.AddRange(startEmailList);
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

        public async Task SaveDb()
        {
            await db.SaveChangesAsync();
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }
}