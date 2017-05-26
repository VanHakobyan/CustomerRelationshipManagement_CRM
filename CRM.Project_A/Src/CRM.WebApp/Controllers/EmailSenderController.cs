using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRM.WebApp.Controllers
{
    public class EmailSenderController : ApiController
    {
        public static void SendEmail(string emailaddress)//List<Contact> list)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress("tsovinar.ghazarian@gmail.com");
                msg.To.Add(emailaddress);
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
