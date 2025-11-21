using Microsoft.AspNetCore.Identity;

namespace POE_Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Department { get; set; }
    }
}
