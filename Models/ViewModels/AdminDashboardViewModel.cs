using SentinelAI.Models;

namespace SentinelAI.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalComplaints { get; set; }
        public int HighRiskCount { get; set; }
        public int CriticalCount { get; set; }
        public int PendingCount { get; set; }
        public int InvestigatingCount { get; set; }
        public int ResolvedCount { get; set; }
        public List<Complaint> AllComplaints { get; set; } = new List<Complaint>();
    }
}
