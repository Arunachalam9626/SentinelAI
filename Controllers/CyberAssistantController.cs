using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelAI.Models.ViewModels;
using SentinelAI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SentinelAI.Controllers
{
    [Authorize]
    public class CyberAssistantController : Controller
    {
        private readonly GeminiService _geminiService;
        private readonly ILogger<CyberAssistantController> _logger;

        public CyberAssistantController(GeminiService geminiService, ILogger<CyberAssistantController> logger)
        {
            _geminiService = geminiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat([FromBody] ChatRequestViewModel request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                    return Json(new { reply = "Please enter a message.", isError = true });

                var reply = await _geminiService.AskCyberAssistantAsync(
                    request.History ?? new List<ChatMessageViewModel>(),
                    request.Message
                );
                return Json(new { reply, isError = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in CyberAssistant.Chat");
                return Json(new { reply = $"Error: {ex.Message}", isError = true });
            }
        }
    }
}
