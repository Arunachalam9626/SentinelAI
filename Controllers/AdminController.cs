using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelAI.Data;
using SentinelAI.Models.ViewModels;

namespace SentinelAI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var complaints = await _context.Complaints
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalComplaints = complaints.Count,
                HighRiskCount = complaints.Count(c => c.RiskLevel == "High"),
                CriticalCount = complaints.Count(c => c.RiskLevel == "Critical"),
                PendingCount = complaints.Count(c => c.Status == "Pending"),
                InvestigatingCount = complaints.Count(c => c.Status == "Investigating"),
                ResolvedCount = complaints.Count(c => c.Status == "Resolved"),
                AllComplaints = complaints
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var validStatuses = new[] { "Pending", "Investigating", "Resolved" };
            if (!validStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "Invalid status value.";
                return RedirectToAction(nameof(Index));
            }

            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null)
            {
                TempData["ErrorMessage"] = "Complaint not found.";
                return RedirectToAction(nameof(Index));
            }

            complaint.Status = status;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin updated complaint {Id} status to {Status}", id, status);

            TempData["SuccessMessage"] = $"Complaint #{id} status updated to '{status}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
