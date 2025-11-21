using System.ComponentModel.DataAnnotations;

namespace POE_Project.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Department { get; set; }
    }
}
