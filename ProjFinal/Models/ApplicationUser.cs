using Microsoft.AspNetCore.Identity;

namespace ProjFinal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
