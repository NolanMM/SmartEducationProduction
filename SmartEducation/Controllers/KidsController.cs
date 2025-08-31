using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.Entities;
using SmartEducation.ViewModels;

namespace SmartEducation.Controllers
{
    [Authorize(Roles = "User, Admin")]
    public class KidsController : Controller
    {
        private readonly SmartEduDbContext _context;
        private readonly UserManager<User> _userManager;

        public KidsController(SmartEduDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // List the kids for the currently logged-in user
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var kids = await _context.Kids
                .Where(k => k.UserId == currentUser.Id)
                .ToListAsync();

            return View(kids);
        }

        // GET: Kids/Create
        public IActionResult Create()
        {
            return View(new CreateKidViewModel());
        }

        // POST: Kids/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateKidViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get the current user
                var currentUser = await _userManager.GetUserAsync(User);

                // Create a new Kid entity from the ViewModel data
                var kid = new Kid
                {
                    Name = model.Name,
                    DateOfBirth = model.DateOfBirth,
                    UserId = currentUser.Id
                };

                _context.Add(kid);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var kid = await _context.Kids.FindAsync(id);

            // Security check: ensure the user owns this kid record
            if (kid == null || kid.UserId != currentUser.Id) return NotFound();

            var model = new EditKidViewModel
            {
                Id = kid.Id,
                Name = kid.Name,
                DateOfBirth = kid.DateOfBirth
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditKidViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                // Retrieve the original kid from DB to ensure the user is not manipulating the UserId
                var kidToUpdate = await _context.Kids.FirstOrDefaultAsync(k => k.Id == id && k.UserId == currentUser.Id);
                // Check if User does not own this kid or kid does not exist
                if (kidToUpdate == null) return NotFound(); 

                kidToUpdate.Name = model.Name;
                kidToUpdate.DateOfBirth = model.DateOfBirth;

                try
                {
                    _context.Update(kidToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Kids.Any(e => e.Id == kidToUpdate.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var kid = await _context.Kids
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUser.Id);

            if (kid == null) return NotFound();

            return View(kid);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var kid = await _context.Kids.FirstOrDefaultAsync(k => k.Id == id && k.UserId == currentUser.Id);

            if (kid == null) return NotFound(); // Security check

            _context.Kids.Remove(kid);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}