using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SentinelAI.Models
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Incident Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
        [Display(Name = "Incident Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Incident Type")]
        public string IncidentType { get; set; } = "Unknown";

        [Display(Name = "Risk Level")]
        public string RiskLevel { get; set; } = "Low";

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Evidence File")]
        public string? EvidencePath { get; set; }

        [Display(Name = "Mitigation Steps")]
        public string MitigationSteps { get; set; } = string.Empty;

        [Display(Name = "Reported Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }
    }
}
