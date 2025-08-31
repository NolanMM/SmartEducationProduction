using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.Entities;
using SmartEducation.Services;

namespace SmartEducation.Controllers
{
    [Authorize(Roles = "Admin, User, OrganizationAdmin")]
    public class ActivityRecommendationController : Controller
    {
        private readonly ActivityRecommendationService _activityRecommendationService;
        private readonly SmartEduDbContext _context;
        private readonly UserManager<User> _userManager;
        public ActivityRecommendationController(ActivityRecommendationService activityRecommendationService, SmartEduDbContext context, UserManager<User> userManager)
        {
            _activityRecommendationService = activityRecommendationService;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRecommendation(string userPrompt)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                ModelState.AddModelError("", "Prompt cannot be empty. Please enter a request.");
                return View("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Redirect to login if user is not found
                return RedirectToAction("LogIn", "Account");
            }

            // Call the service to get recommendations from the OpenAI API.
            var recommendations = await _activityRecommendationService.GetActivityRecommendationsAsync(userPrompt, user);

            if (recommendations != null && recommendations.Any())
            {
                _context.ActivityRecommendations.AddRange(recommendations);
                await _context.SaveChangesAsync();
            }

            // Pass the list of recommendations to the result view.
            return View("RecommendationResult", recommendations);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Retrieve all recommendations for the current user from the database.
            var userRecommendations = await _context.ActivityRecommendations
                                            .Where(r => r.UserId == user.Id)
                                            .OrderByDescending(r => r.CreatedAt)
                                            .ToListAsync();

            return View(userRecommendations);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Find the specific recommendation by its ID.
            var recommendation = await _context.ActivityRecommendations
                .FirstOrDefaultAsync(m => m.Id == id);

            if (recommendation == null)
            {
                return NotFound();
            }

            // Security check: ensure the user owns the recommendation unless they are an Admin.
            if (recommendation.UserId != user.Id && !User.IsInRole("Admin"))
            {
                // Return a 403 Forbidden error.
                return Forbid(); 
            }

            return View(recommendation);
        }


    }
}
