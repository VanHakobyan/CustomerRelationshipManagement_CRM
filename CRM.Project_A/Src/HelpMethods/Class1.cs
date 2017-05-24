using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic.FileIO;
using EntityLibrary;
using System.Windows.Forms;

namespace HelpMethods
{

    public static class ReadExceCSVFile
    {
        static List<InputType> ReadExcel<InputType>(string path)
        {
            Excel.Application xlApp = null;
            Excel.Workbook xlWorkBook = null;
            Excel.Worksheet xlWorkSheet = null;
            Excel.Range range;

            try
            {
                string str = string.Empty;
                int rCnt;
                int rw = 0;
                int cl = 0;

                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(path, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                range = xlWorkSheet.UsedRange;
                rw = range.Rows.Count;
                cl = range.Columns.Count;

                if (typeof(InputType) == typeof(Contact))
                {
                    List<Contact> result = new List<Contact>();
                    Contact temp;
                    for (rCnt = 1; rCnt <= rw; rCnt++)
                    {
                        temp = new Contact();
                        temp.ContactId = (int)range.Cells[rCnt, 1].Value2;
                        temp.FullName = (string)range.Cells[rCnt, 2].Value2;
                        temp.CompanyName = (string)range.Cells[rCnt, 3].Value2;
                        temp.Position = (string)range.Cells[rCnt, 4].Value2;
                        temp.Country = (string)range.Cells[rCnt, 5].Value2;
                        temp.Email = (string)range.Cells[rCnt, 6].Value2;
                        result.Add(temp);
                    }
                    return result as List<InputType>;
                }
                else
                    if (typeof(InputType) == typeof(EmailList))
                {
                    List<EmailList> result = new List<EmailList>();
                    EmailList temp;
                    for (rCnt = 1; rCnt <= rw; rCnt++)
                    {
                        temp = new EmailList();
                        temp.EmailListID = (int)range.Cells[rCnt, 1].Value2;
                        temp.EmailListName = (string)range.Cells[rCnt, 2].Value2;
                        result.Add(temp);
                    }
                    return result as List<InputType>;
                }
                else
                    if (typeof(InputType) == typeof(Template))
                {
                    List<Template> result = new List<Template>();
                    Template temp;
                    for (rCnt = 1; rCnt <= rw; rCnt++)
                    {
                        temp = new Template();
                        temp.TemplateId = (int)range.Cells[rCnt, 1].Value2;
                        temp.TemplateName = (string)range.Cells[rCnt, 2].Value2;
                        result.Add(temp);
                    }
                    return result as List<InputType>;
                }


                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                xlWorkBook.Close(true, null, null);
                xlApp.Quit();

                Marshal.ReleaseComObject(xlWorkSheet);
                Marshal.ReleaseComObject(xlWorkBook);
                Marshal.ReleaseComObject(xlApp);
            }
        }


        static List<InputType> ReadCSV<InputType>(string path)
        {
            using (TextFieldParser parser = new TextFieldParser(path))
            {
                try
                {
                    parser.TextFieldType = FieldType.Delimited;
                    string str;
                    string[] splitedRow;
                    if (typeof(InputType) == typeof(Contact))
                    {
                        List<Contact> result = new List<Contact>();
                        Contact temp;
                        while (!parser.EndOfData)
                        {
                            str = parser.ReadLine();
                            splitedRow = str.Split(',', ';');
                            temp = new Contact();
                            temp.ContactId = int.Parse(splitedRow[1]);
                            temp.FullName = splitedRow[2];
                            temp.CompanyName = splitedRow[3];
                            temp.Position = splitedRow[4];
                            temp.Country = splitedRow[5];
                            temp.Email = splitedRow[6];
                            result.Add(temp);
                        }
                        return result as List<InputType>;
                    }
                    else
                        if (typeof(InputType) == typeof(EmailList))
                    {
                        List<EmailList> result = new List<EmailList>();
                        EmailList temp;
                        while (!parser.EndOfData)
                        {
                            str = parser.ReadLine();
                            splitedRow = str.Split(',', ';');
                            temp = new EmailList();
                            temp.EmailListID = int.Parse(splitedRow[1]);
                            temp.EmailListName = splitedRow[2];

                            result.Add(temp);
                        }
                        return result as List<InputType>;
                    }
                    else
                        if (typeof(InputType) == typeof(Template))
                    {
                        List<Template> result = new List<Template>();
                        Template temp;
                        while (!parser.EndOfData)
                        {
                            str = parser.ReadLine();
                            splitedRow = str.Split(',', ';');
                            temp = new Template();
                            temp.TemplateId = int.Parse(splitedRow[1]);
                            temp.TemplateName = splitedRow[2];

                            result.Add(temp);
                        }
                        return result as List<InputType>;
                    }

                    return null;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return null;
                }
            }

        }


    }
}
