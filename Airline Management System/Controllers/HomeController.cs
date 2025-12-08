using Airline_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Airline_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // 1. The Landing Page (Entry Point)
        public IActionResult Index()
        {
            // If I am an Admin, Send me straight to work.
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("AdminDashboard");
            }

            // If I am a Guest or a Customer, show the Landing Page.
            return View();
        }

        // 2. The Admin Dashboard (Protected)
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
