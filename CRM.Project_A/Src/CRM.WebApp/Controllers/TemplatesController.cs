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

namespace CRM.WebApp.Controllers
{
    public class TemplatesController : ApiController
    {
        private ApplicationManager manager = new ApplicationManager();
        // GET: api/Templates
        public async Task<List<TemplateResponseModel>> GetTemplates()
        {
            return await manager.GetTemplates();
        }
        public async Task<bool> TemplateExistsAsync(int id)
        {
            return await manager.TemplateExistsAsync(id);
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