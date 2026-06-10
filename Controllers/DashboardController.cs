using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelAI.Data;
using SentinelAI.Models.ViewModels;

namespace SentinelAI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var myComplaints = await _context.Complaints
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalComplaints = myComplaints.Count,
                HighRiskCount = myComplaints.Count(c => c.RiskLevel == "High"),
                CriticalCount = myComplaints.Count(c => c.RiskLevel == "Critical"),
                PendingCount = myComplaints.Count(c => c.Status == "Pending"),
                ResolvedCount = myComplaints.Count(c => c.Status == "Resolved"),
                RecentComplaints = myComplaints.Take(5).ToList()
            };

            return View(viewModel);
        }
    }
}
