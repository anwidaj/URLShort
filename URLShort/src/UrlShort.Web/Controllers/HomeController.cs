using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UrlShort.Web.Models;

namespace UrlShort.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Url");
        }

        return View();
    }
}
