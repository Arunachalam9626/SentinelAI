using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelAI.Data;
using SentinelAI.Models;
using SentinelAI.Services;

namespace SentinelAI.Controllers
{
    [Authorize]
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly GeminiService _geminiService;
        private readonly FileUploadService _fileUploadService;
        private readonly ILogger<ComplaintsController> _logger;

        public ComplaintsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            GeminiService geminiService,
            FileUploadService fileUploadService,
            ILogger<ComplaintsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _geminiService = geminiService;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        // GET: Complaints (My Complaints)
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var complaints = await _context.Complaints
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
            return View(complaints);
        }

        // GET: Complaints/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Complaints/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] Complaint complaint, IFormFile? evidenceFile)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("IncidentType");
            ModelState.Remove("RiskLevel");
            ModelState.Remove("Status");
            ModelState.Remove("MitigationSteps");

            if (!ModelState.IsValid)
                return View(complaint);

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Unable to identify user. Please log in again.");
                return View(complaint);
            }

            complaint.UserId = userId;
            complaint.CreatedDate = DateTime.UtcNow;
            complaint.Status = "Pending";

            // Handle file upload
            if (evidenceFile != null && evidenceFile.Length > 0)
            {
                try
                {
                    complaint.EvidencePath = await _fileUploadService.SaveFileAsync(evidenceFile);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("evidenceFile", ex.Message);
                    return View(complaint);
                }
            }

            // Save first to get an Id
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            // AI Analysis
            try
            {
                var analysis = await _geminiService.AnalyzeComplaintAsync(complaint.Description);
                complaint.IncidentType = analysis.IncidentType;
                complaint.RiskLevel = analysis.RiskLevel;
                complaint.MitigationSteps = analysis.MitigationSteps;
                await _context.SaveChangesAsync();

                // Create Alert for High/Critical
                if (analysis.RiskLevel == "High" || analysis.RiskLevel == "Critical")
                {
                    var alert = new Alert
                    {
                        ComplaintId = complaint.Id,
                        Message = $"⚠️ {analysis.RiskLevel} risk incident reported: {complaint.Title} — Immediate action required.",
                        CreatedDate = DateTime.UtcNow,
                        IsRead = false
                    };
                    _context.Alerts.Add(alert);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI analysis failed for complaint {Id}", complaint.Id);
                // Still save with defaults
            }

            TempData["SuccessMessage"] = $"Incident reported successfully. AI Classification: {complaint.IncidentType} | Risk: {complaint.RiskLevel}";
            return RedirectToAction(nameof(Index));
        }

        // GET: Complaints/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var complaint = await _context.Complaints
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null) return NotFound();

            // Non-admin users can only view their own complaints
            if (!isAdmin && complaint.UserId != userId)
                return Forbid();

            return View(complaint);
        }

        // GET: Complaints/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (complaint == null) return NotFound();

            return View(complaint);
        }

        // POST: Complaints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var complaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (complaint == null) return NotFound();

            // Delete associated file
            if (!string.IsNullOrEmpty(complaint.EvidencePath))
                _fileUploadService.DeleteFile(complaint.EvidencePath);

            // Delete associated alerts
            var alerts = _context.Alerts.Where(a => a.ComplaintId == id);
            _context.Alerts.RemoveRange(alerts);

            _context.Complaints.Remove(complaint);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Incident report deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
