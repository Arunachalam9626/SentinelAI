using System.ComponentModel.DataAnnotations;

namespace SentinelAI.Models
{
    public class Mitigation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string IncidentType { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Steps { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
