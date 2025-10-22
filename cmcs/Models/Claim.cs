using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cmcs.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }

        [Required]
        [Display(Name = "Claim Month")]
        public DateTime ClaimMonth { get; set; }

        [Required]
        [Range(0.5, 200, ErrorMessage = "Hours must be between 0.5 and 200")]
        [Display(Name = "Total Hours")]
        public decimal TotalHours { get; set; }

        [Required]
        [Range(100, 1000, ErrorMessage = "Rate must be between R100 and R1000")]
        [Display(Name = "Hourly Rate")]
        public decimal RatePerHour { get; set; }

        [Display(Name = "Total Amount")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalAmount => TotalHours * RatePerHour;

        [Required]
        [StringLength(500)]
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; } = string.Empty;

        public string Status { get; set; } = "Submitted";

        [Display(Name = "Submitted Date")]
        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        [Display(Name = "Lecturer Name")]
        public string LecturerName { get; set; } = string.Empty;

        [Display(Name = "Lecturer Email")]
        public string LecturerEmail { get; set; } = string.Empty;

        public virtual List<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();
    }
}