using System.Security.Claims;

namespace POE_Project.Models
{
    public class Lecturer
    {
        public int LecturerID { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public List<Claim> Claims { get; set; } = new List<Claim>();
    }
}

