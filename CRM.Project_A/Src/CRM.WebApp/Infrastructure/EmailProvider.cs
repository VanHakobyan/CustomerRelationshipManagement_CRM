using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace CRM.WebApp.Infrastructure
{
    public static class EmailProvider
    {
      
        public static void SendEmail(List<Contact> list, int TemplateID)//List<Contact> list)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress("tsovinar.ghazarian@gmail.com");
                foreach (var item in list)
                {
                    msg.To.Add(item.Email);
                }
                msg.Subject = "Heloo API";
                msg.Body = "Ba chimacar";
                SmtpClient client =
                new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("tsovinar.ghazarian@gmail.com", "123345667899")
                };

                try
                {
                    client.Send(msg);
                }
                catch (Exception ex)
                {
                    throw;

                }
            }
        }
    }
}