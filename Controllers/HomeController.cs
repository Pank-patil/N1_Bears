using Microsoft.AspNetCore.Mvc;
using Unique.Models;
using System;
using Microsoft.AspNetCore.Http;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    //// GET: Show the form
    //[HttpGet]
    //public IActionResult JoinWL()
    //{
    //    var isVerified = HttpContext.Session.GetString("TwitterVerified");
    //    ViewBag.TwitterVerified = isVerified == "true";

    //    // Pass empty model for form binding
    //    return View(new WhitelistEntry());
    //}
    public IActionResult JoinWL()
    {
        var isVerified = HttpContext.Session.GetString("TwitterVerified");
        ViewBag.TwitterVerified = isVerified == "true";
        return View(new WhitelistEntry()); // ? always send model to avoid null error in View
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult JoinWL(WhitelistEntry entry)
    {
        try
        {
            var isVerified = HttpContext.Session.GetString("TwitterVerified");
            ViewBag.TwitterVerified = isVerified == "true";

            if (ModelState.IsValid)
            {
                // Always generate a new referral code
                entry.ReferralCode = GenerateReferralCode();

                // Only assign ReferredBy if ReferralCodeInput was filled
                if (!string.IsNullOrWhiteSpace(entry.ReferralCodeInput))
                {
                    entry.ReferredBy = entry.ReferralCodeInput;
                }

                _context.WhitelistEntries.Add(entry);
                _context.SaveChanges();

                ViewBag.BaseUrl = $"{Request.Scheme}://{Request.Host}";
                return View("Sucess", entry);
            }

            return View("JoinWL", entry);
        }
        catch (Exception)
        {
            return RedirectToAction("Error", "Home", new { requestId = HttpContext.TraceIdentifier });
        }
    }



    // Home page
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Error(string requestId = null)
    {
        return View("Error", new ErrorViewModel
        {
            RequestId = requestId ?? HttpContext.TraceIdentifier

        });
    }

    private string GenerateReferralCode()
    {
        // Generate 8-char alphanumeric referral code
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}
