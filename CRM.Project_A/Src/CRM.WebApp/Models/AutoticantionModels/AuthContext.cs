using Microsoft.AspNet.Identity.EntityFramework;

namespace CRM.WebApp.Models.AutoticantionModels
{
    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        public AuthContext()
            : base("AuthContext")
        {

        }
    }
}