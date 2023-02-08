using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ImageResizeWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ValidateAppInsightsEvents()
        {
            var eventMessage = $"Test events: Validate App Insights Event logged at {DateTime.Now}";
            
            //TODO: Exercise 1: Make sure this logs to application Insights correctly as an Event or Trace
            //                  HINT: use an additional call and leave the rest of the code alone
            _logger.LogError(eventMessage);

            return RedirectToAction("Index");
        }

        public IActionResult ValidateAppInsightsExceptions()
        {
            try
            {
                throw new Exception("Validate Exception is caught");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Test exception thrown at {DateTime.Now} | {ex.Message}";
                //TODO: Exercise 1: Make sure this logs to application Insights as an Exception
                //                  HINT: use an additional call and leave the rest of the code alone

                _logger.LogError(errorMessage);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
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