using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;

namespace CRM.WebApp
{
    public class ExceptionCustomFilterAttribute:ExceptionFilterAttribute
    {
        LoggerManager logger = new LoggerManager();
        public override Task OnExceptionAsync(HttpActionExecutedContext context,CancellationToken token)
        {
            logger.LogError(context.Exception, context.Request.Method, context.Request.RequestUri);
            if (context.Exception is NotImplementedException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented);
            }
            if (context.Exception is DbException)
            {
                var x = new HttpResponseMessage
                {
                    //not implementacion 
                };
            }
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("An unhandled exception was thrown by service"),  
                    ReasonPhrase = "Internal Server Error.Please Contact your Administrator."
            };
            return base.OnExceptionAsync(context, token);
        }
    }
}