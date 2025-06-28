using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FanRide.Controllers
{
    public class RidersController : Controller
    {
        public IActionResult Dashboard()
        {
            return View(); // Will look for Views/Riders/Dashboard.cshtml
        }
    }
}
