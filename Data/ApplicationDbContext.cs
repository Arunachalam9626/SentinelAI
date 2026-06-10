using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SentinelAI.Models;

namespace SentinelAI.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Mitigation> Mitigations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Mitigations (no FK dependency, safe to seed here)
            builder.Entity<Mitigation>().HasData(
                new Mitigation
                {
                    Id = 1,
                    IncidentType = "Phishing",
                    Steps = "Do not click suspicious links. Report to CERT. Change all passwords. Enable MFA immediately.",
                    LastUpdated = new DateTime(2025, 1, 1)
                },
                new Mitigation
                {
                    Id = 2,
                    IncidentType = "Fraud",
                    Steps = "Freeze accounts. Notify financial officer. File a cyber complaint. Preserve evidence.",
                    LastUpdated = new DateTime(2025, 1, 1)
                },
                new Mitigation
                {
                    Id = 3,
                    IncidentType = "Malware",
                    Steps = "Isolate system. Run full scan. Reimage if necessary. Report to CERT. Update all software.",
                    LastUpdated = new DateTime(2025, 1, 1)
                },
                new Mitigation
                {
                    Id = 4,
                    IncidentType = "Espionage",
                    Steps = "Initiate counterintelligence. Lock all access. Brief commanding officer. Preserve all digital evidence.",
                    LastUpdated = new DateTime(2025, 1, 1)
                }
            );
        }
    }
}
