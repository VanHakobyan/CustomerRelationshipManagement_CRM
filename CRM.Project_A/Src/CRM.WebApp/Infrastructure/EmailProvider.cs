using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.IO;
using System.Threading.Tasks;
using CRM.WebApp.Models;

namespace CRM.WebApp.Infrastructure
{
    public class EmailProvider
    {
        DataBaseCRMEntityes db = new DataBaseCRMEntityes();
        private string GetMessageText(int templateId, ContactResponseModel contact)
        {
            //var template = await db.Templates.FindAsync(templateId);
            var template = db.Templates.Find(templateId);
            string path = HttpContext.Current?.Request.MapPath(template.Path);
            var templateText = File.ReadAllText(path);
            return
                templateText.Replace("{FullName}", contact.FullName)
                    .Replace("{CompanyName}", contact.CompanyName)
                    .Replace("{Position}", contact.Position)
                    .Replace("{Country}", contact.Country)
                    .Replace("{Email}", contact.Email)
                    .Replace("{DateTimeNow}", DateTime.UtcNow.ToString());
        }

        public void SendEmail(ContactResponseModel contact, int TemplateID)//List<Contact> list)
        {

            using (MailMessage msg = new MailMessage())
            {

                msg.From = new MailAddress("vanhakobyan1996@gmail.com");
                msg.To.Add(contact.Email);
                msg.Subject = "BetConstruct Team BETA";
                msg.IsBodyHtml = true;
                msg.Body = GetMessageText(TemplateID, contact);
                SmtpClient client =
                new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("vanhakobyan1996@gmail.com", "VAN606580")
                };

                try
                {
                    client.Send(msg);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);

                }

            }

        }

        public void SendEmailList(List<ContactResponseModel> list, int TemplateID)
        {
            foreach (var contact in list)
                SendEmail(contact, TemplateID);
        }

        //    private EmailContent GetEmailTemplate(Template template, Contact contact)
        //    {
        //        StringBuilder pathHtML = new StringBuilder(@"../../Templates/");
        //        StringBuilder pathImage = new StringBuilder(@"../../Templates/images");
        //        string contentHTML;
        //        byte[] contentImage;
        //        StringBuilder builder = null;
        //        switch (template)
        //        {
        //            case Template.anniversary:
        //                {
        //                    pathHtML.Append("anniversary.html");
        //                    pathImage.Append("anniversary.jpg");
        //                }

        //                break;
        //            case Template.birthday:
        //                {
        //                    pathHtML.Append("birthday.html");
        //                    pathImage.Append("birthday.jpg");
        //                }
        //                break;
        //            case Template.christmas:
        //                {
        //                    pathHtML.Append("christmas.html");
        //                    pathImage.Append("christmas.jpg");
        //                }
        //                break;
        //            default:
        //                break;
        //        }

        //        contentHTML = File.ReadAllText(pathHtML.ToString());
        //        builder = new StringBuilder(contentHTML);

        //        builder = builder.Replace("{FullName}", contact.FullName);
        //        builder = builder.Replace("{CompanyName}", contact.CompanyName);
        //        builder = builder.Replace("{Position}", contact.Position);
        //        builder = builder.Replace("{Country}", contact.Country);
        //        builder = builder.Replace("{DateTimeNow}", DateTime.Now.ToString("MMMM dd, yyyy"));

        //        contentImage = System.IO.File.ReadAllBytes(pathImage.ToString());

        //        return new EmailContent()
        //        {
        //            html = builder.ToString(),
        //            Image = contentImage
        //        };
        //    }

        //}

        //public class EmailContent
        //{
        //    public string html;
        //    public byte[] Image;
        //}
        //public enum Template
        //{
        //    anniversary,
        //    birthday,
        //    christmas
        //}
    }
}