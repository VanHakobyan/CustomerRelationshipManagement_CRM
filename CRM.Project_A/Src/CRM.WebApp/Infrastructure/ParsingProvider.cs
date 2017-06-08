using CRM.WebApp.Models;
using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations;

namespace CRM.WebApp.Infrastructure
{
    public class ParsingProvider
    {
        private readonly DataBaseCRMEntityes db = new DataBaseCRMEntityes();
        private readonly ModelFactory factory = new ModelFactory();

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

    }
}