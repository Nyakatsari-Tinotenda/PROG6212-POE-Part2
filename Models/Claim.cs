
    using System.ComponentModel.DataAnnotations;
    using System.Reflection.Metadata;

    namespace POE_Project.Models
{
        public class Claim
        {
            public int ClaimID { get; set; }
            [Required]
            public double HoursWorked { get; set; }
            [Required]
            public double HourlyRate { get; set; }
        public double TotalAmount { get; set; }
     
            public string Status { get; set; } = "Pending";
            public string? Notes { get; set; }

            public int LecturerID { get; set; }
            public Lecturer? Lecturer { get; set; }
            public List<Document> Documents { get; set; } = new List<Document>();
        }
    }
