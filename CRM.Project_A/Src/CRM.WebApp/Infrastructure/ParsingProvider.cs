using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using EntityLibrary;
using LinqToExcel;

namespace CRM.WebApp.Infrastructure
{
    public class ParsingProvider
    {
        public static List<ResultType> ParseExcel<ResultType>(byte[] array)
        {
            File.WriteAllBytes(@"D:\TestExel.xlsx", array);

            var pathToExcelFile = @"D:\TestExel.xlsx";
            var sheetName = @"Sheet";
            var excelFile = new ExcelQueryFactory(pathToExcelFile);

            List<Row> rowsList = null;
            try
            {
                rowsList = excelFile.Worksheet(sheetName).ToList();
            }
            catch (Exception)
            {
                File.Delete(@"D:\TestExel.xlsx");
                File.WriteAllBytes(@"D:\TestExel.csv", array);
                pathToExcelFile = @"D:\TestExel.csv";
                excelFile = new ExcelQueryFactory(pathToExcelFile);
                rowsList = excelFile.Worksheet(sheetName).ToList();
            }

            if (typeof(ResultType) == typeof(Contact))
            {
                var listOfContacts =
                rowsList.Select(
                contact =>
                 new Contact
                 {
                     ContactId = int.Parse(contact["ContactId"]),
                     FullName = contact["FullName"],
                     CompanyName = contact["CompanyName"],
                     Position = contact["Position"],
                     Country = contact["Country"],
                     Email = contact["Email"]
                 }) as List<ResultType>;

                return listOfContacts;
            }
            else
              if (typeof(ResultType) == typeof(EmailList))
            {
                var listOfContacts =
                rowsList.Select(
                emailList =>
                 new EmailList
                 {
                     EmailListID = int.Parse(emailList["EmailListID"]),
                     EmailListName = emailList["CompanyName"],
                 }) as List<ResultType>;

                return listOfContacts;
            }
            else
            {
                if (typeof(ResultType) == typeof(Template))
                {
                    var listOfContacts =
                    rowsList.Select(
                    template =>
                     new Template
                     {
                         TemplateId = int.Parse(template["TemplateId"]),
                         TemplateName = template["TemplateName"],
                     }) as List<ResultType>;

                    return listOfContacts;
                }
            }

            return null;

        }
    }
}