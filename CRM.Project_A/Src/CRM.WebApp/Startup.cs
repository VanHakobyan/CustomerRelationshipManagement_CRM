using CRM.WebApp.Infrastructure;
using CRM.WebApp.Models;
using CRM.WebApp.Provider;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;

[assembly: OwinStartup(typeof(CRM.WebApp.Startup))]

namespace CRM.WebApp
{
    public class Startup
    {
        //public void Configuration(IAppBuilder app)
        //{
        //    app.UseWelcomePage("/");

        //    HttpConfiguration httpConfig = new HttpConfiguration();

        //    ConfigureWebApi(httpConfig);

        //    app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

        //    app.UseWebApi(httpConfig);
        //}
        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuth(app);
            app.UseWelcomePage("/");
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);
        }
        private void ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            //var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            //jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        //private void ConfigureOAuth(IAppBuilder app)
        //{
        //    app.CreatePerOwinContext(ApplicationDbContext.Create);
        //    app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

        //    var options = new OAuthAuthorizationServerOptions
        //    {
        //        AllowInsecureHttp = true,
        //        TokenEndpointPath = new PathString("/api/token"),
        //        AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
        //        Provider = new ApplicationOAuthProvider()
        //    };

        //    app.UseOAuthAuthorizationServer(options);
        //    app.UseOAuthBearerAuthentication
        //    (
        //        new OAuthBearerAuthenticationOptions
        //        {
        //            Provider = new OAuthBearerAuthenticationProvider()
        //        }
        //    );
        //}
       

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new SimpleAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

        }
    }
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            //jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}