using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelAI.Models
{
    public class Alert
    {
        [Key]
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        [ForeignKey("ComplaintId")]
        public virtual Complaint? Complaint { get; set; }
    }
}
