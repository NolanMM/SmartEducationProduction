using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.ViewModels;

namespace SmartEducation.Controllers;

public class HomeController : Controller
{
    private readonly SmartEduDbContext _context;
    private readonly ILogger<HomeController> _logger;
    
    public HomeController(ILogger<HomeController> logger, SmartEduDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = new StandardViewModel
        {
            GradeStandards = await _context.NGSS_Standard.ToListAsync(),
            NgssDetailedStandards = await _context.NGSS_Detailed_Standard.ToListAsync()
        };
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Aboutus()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
