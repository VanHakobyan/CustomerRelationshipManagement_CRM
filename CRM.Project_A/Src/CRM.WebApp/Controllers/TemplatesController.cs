using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using EntityLibrary;
using System.Threading.Tasks;
using CRM.WebApp.Infrastructure;
using CRM.WebApp.Models;
using System.Net.Http.Headers;


namespace CRM.WebApp.Controllers
{
    public class TemplatesController : ApiController
    {
        private readonly ApplicationManager manager = new ApplicationManager();
        private readonly LoggerManager logger = new LoggerManager();
        // GET: api/Templates
        public async Task<List<TemplateResponseModel>> GetTemplates()
        {
            return await manager.GetTemplates();
        }
        public async Task<bool> TemplateExistsAsync(int id)
        {
            return await manager.TemplateExistsAsync(id);
        }
        [Route("api/Templates/errors")]
        public HttpResponseMessage GetLog()
        {
            var response = new HttpResponseMessage { Content = new StringContent(logger.LoggerErrors()) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                manager.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}