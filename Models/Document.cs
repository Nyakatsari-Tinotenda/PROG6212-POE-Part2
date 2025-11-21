namespace POE_Project.Models
{

    public class Document
    {
        public int DocumentID { get; set; }
        public string FileName { get; set; }
        public string StoredFileName { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public long SizeBytes { get; set; }
        public int ClaimID { get; set; }
        public Claim Claim { get; set; }
    }
}
