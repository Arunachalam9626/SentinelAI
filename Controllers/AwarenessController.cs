using Microsoft.AspNetCore.Mvc;

namespace SentinelAI.Controllers
{
    public class AwarenessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Phishing()
        {
            return View();
        }

        public IActionResult Malware()
        {
            return View();
        }

        public IActionResult SocialEngineering()
        {
            return View();
        }

        public IActionResult DefenceOpsec()
        {
            return View();
        }
    }
}
