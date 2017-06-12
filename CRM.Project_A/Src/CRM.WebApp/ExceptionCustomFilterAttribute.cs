using System;
using System.Data;
using System.Data.Entity.Core;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace CRM.WebApp
{
    public class ExceptionCustomFilterAttribute : ExceptionFilterAttribute
    {
        private readonly LoggerManager logger = new LoggerManager();
        public override Task OnExceptionAsync(HttpActionExecutedContext context, CancellationToken token)
        {
            logger.LogError(context.Exception, context.Request.Method, context.Request.RequestUri);

            if (context.Exception is NullReferenceException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(string.Format($"{context.Exception.Message}\n{context.Exception.InnerException?.Message}")),
                    ReasonPhrase = "Bad Request"
                };
            }
            else if (context.Exception is DataException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent(string.Format($"{context.Exception.Message}\n{context.Exception.InnerException?.Message}")),
                    ReasonPhrase = "throwed DataBase Exception"
                };
            }

            else if (context.Exception is EntityException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent(string.Format($"{context.Exception.Message}\n{context.Exception.InnerException?.Message}")),
                    ReasonPhrase = "throwed EF Exception"
                };
            }

            else if (context.Exception is NotImplementedException)
            {

                context.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented)
                {
                    Content = new StringContent(string.Format($"{context.Exception.Message}\n{context.Exception.InnerException?.Message}")),
                    ReasonPhrase = "throwed Not Implemented Exception",
                };
            }
            else if (context.Exception is FileNotFoundException )
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented)
                {
                    Content = new StringContent(string.Format($"{context.Exception.Message}\n{context.Exception.InnerException?.Message}")),
                    ReasonPhrase = "Invalid Type of File"
                };
            }
            else if (context.Exception is IndexOutOfRangeException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented)
                {
                    Content = new StringContent(string.Format($"{context.Exception.Message}\n{context.Exception.InnerException?.Message}")),
                    ReasonPhrase = "Incorrect file"
                };
            }
            else if (context.Exception is IOException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent(string.Format($"{context.Exception.Message}\n{context.Exception.InnerException?.Message}")),
                    ReasonPhrase = "throwed IO Exception"
                };
            }
            else
            {

                context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(string.Format($"{context.Exception.Message}\n{context.Exception.InnerException?.Message}")),
                    ReasonPhrase = "Internal Server Error. Please Contact your Administrator."
                };

            }
            return base.OnExceptionAsync(context, token);
        }
    }
}