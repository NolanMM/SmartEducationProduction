using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.ViewModels;

namespace SmartEducation.Controllers
{
    public class StandardController : Controller
    {
        private readonly SmartEduDbContext _context;

        public StandardController(SmartEduDbContext context)
        {
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

        [HttpGet]
        public async Task<IActionResult> GetGradeStandardDetails(long id)
        {
            var gradeStandard = await _context.NGSS_Standard.FindAsync(id);
            if (gradeStandard == null)
            {
                return NotFound();
            }
            return PartialView("GradeStandardDetailsPartial", gradeStandard);
        }

        [HttpGet]
        public async Task<IActionResult> GetNgssDetailedStandardDetails(int id)
        {
            var ngssStandard = await _context.NGSS_Detailed_Standard.FindAsync(id);
            if (ngssStandard == null)
            {
                return NotFound();
            }
            return PartialView("NgssDetailedStandardDetailsPartial", ngssStandard);
        }
    }
}
