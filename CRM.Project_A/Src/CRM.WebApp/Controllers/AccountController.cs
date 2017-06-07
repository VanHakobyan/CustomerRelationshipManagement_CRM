using CRM.WebApp.Infrastructure;
using CRM.WebApp.Models;
using CRM.WebApp.Models.AutoticantionModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CRM.WebApp.Controllers
{
    public class AccountController : ApiController
    {
        ApplicationUserManager manager = HttpContext.Current.GetOwinContext().Get<ApplicationUserManager>();

        [AllowAnonymous]
        [Route("api/account/register")]
        public async Task<IHttpActionResult> PostRegister([FromBody]RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email
            };


            IdentityResult result = await manager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.ToString());
            }

            return Ok();
        }
    }
}