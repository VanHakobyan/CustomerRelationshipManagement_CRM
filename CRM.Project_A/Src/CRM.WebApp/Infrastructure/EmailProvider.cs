using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace CRM.WebApp.Infrastructure
{
    public class EmailProvider
    {
        public void SendEmail(List<Contact> list, int TemplateID)//List<Contact> list)
        {
            using (MailMessage msg = new MailMessage())
            {
               
                msg.From = new MailAddress("vanhakobyan1996@gmail.com");
                foreach (var item in list)
                {
                    msg.To.Add(item.Email);
                }
                msg.Subject = "Heloo API";
                msg.Body = "First Step";
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
                    throw new  Exception(ex.Message);

                }
               
            }
           
        }
       
    }
}