using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cmcs.Models
{
    public class SupportingDocument
    {
        public int SupportingDocumentId { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public int ClaimId { get; set; }

        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }
    }
}