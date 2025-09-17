namespace cmcs.Models
{
    public class Claim
    {
        // These properties are for display purposes only in the views
        public int ClaimId { get; set; }
        public DateTime ClaimMonth { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Submitted"; // Default status
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
    }
}