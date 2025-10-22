namespace POE_Project.Models
{
    
        public class Document
        {
            public int DocumentID { get; set; }
            public string FileName { get; set; } = "";
            public string FilePath { get; set; } = "";
            public int ClaimID { get; set; }
            public Claim? Claim { get; set; }
        }
    }
