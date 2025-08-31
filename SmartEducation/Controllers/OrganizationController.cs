using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.Entities;
using SmartEducation.ViewModels;

namespace SmartEducation.Controllers
{
    [Authorize(Roles = "OrganizationAdmin")]
    public class OrganizationController : Controller
    {
        private readonly SmartEduDbContext _context;
        private readonly UserManager<User> _userManager;

        public OrganizationController(SmartEduDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard for the organization admin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.OrganizationId == null)
            {
                return Content("Error: You are not assigned to an organization.");
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == currentUser.OrganizationId);

            if (organization == null) return NotFound();

            return View(organization);
        }

        // Manage users within this admin's organization
        [HttpGet]
        public async Task<IActionResult> MyUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var usersInOrg = await _context.Users
                .Where(u => u.OrganizationId == currentUser.OrganizationId)
                .ToListAsync();

            return View(usersInOrg);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var userToEdit = await _userManager.FindByIdAsync(id);

            // Security check: Ensure the user being edited belongs to the admin's organization
            if (userToEdit == null || userToEdit.OrganizationId != currentUser.OrganizationId)
            {
                return Forbid();
            }

            var model = new OrgEditUserViewModel
            {
                Id = userToEdit.Id,
                Email = userToEdit.Email
            };

            if (await _userManager.IsInRoleAsync(userToEdit, "Admin"))
            {
                ModelState.AddModelError("", "Cannot edit an Admin user.");
                return RedirectToAction(nameof(MyUsers));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(OrgEditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var currentUser = await _userManager.GetUserAsync(User);
            var userToUpdate = await _userManager.FindByIdAsync(model.Id);

            if (userToUpdate == null || userToUpdate.OrganizationId != currentUser.OrganizationId)
            {
                return Forbid();
            }

            if (await _userManager.IsInRoleAsync(userToUpdate, "Admin"))
            {
                ModelState.AddModelError("", "Cannot edit an Admin user.");
                return View(model);
            }

            userToUpdate.Email = model.Email;
            userToUpdate.UserName = model.Email;
            var updateResult = await _userManager.UpdateAsync(userToUpdate);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors) ModelState.AddModelError("", error.Description);
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(userToUpdate);
                var passwordResult = await _userManager.ResetPasswordAsync(userToUpdate, token, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors) ModelState.AddModelError("", error.Description);
                    return View(model);
                }
            }

            return RedirectToAction(nameof(MyUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var userToToggle = await _userManager.FindByIdAsync(id);

            // Security check
            if (userToToggle == null || userToToggle.OrganizationId != currentUser.OrganizationId)
            {
                return Forbid();
            }

            // Prevent the admin from deactivating themselves
            if (userToToggle.Id == currentUser.Id)
            {
                return RedirectToAction(nameof(MyUsers));
            }

            if (await _userManager.IsInRoleAsync(userToToggle, "Admin"))
            {
                return RedirectToAction(nameof(MyUsers));
            }

            if (await _userManager.IsLockedOutAsync(userToToggle))
            {
                await _userManager.SetLockoutEndDateAsync(userToToggle, null);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(userToToggle, DateTimeOffset.MaxValue);
            }

            return RedirectToAction(nameof(MyUsers));
        }

        [HttpGet]
        public async Task<IActionResult> EditOrganizationInfo()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var organization = await _context.Organizations.FindAsync(currentUser.OrganizationId);
            if (organization == null) return NotFound();

            var model = new EditOrganizationInfoViewModel
            {
                Id = organization.Id,
                Name = organization.Name
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrganizationInfo(EditOrganizationInfoViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var currentUser = await _userManager.GetUserAsync(User);
            var organizationToUpdate = await _context.Organizations.FindAsync(model.Id);

            // Ensure admin is editing their own organization (for Security check)
            if (organizationToUpdate == null || organizationToUpdate.Id != currentUser.OrganizationId)
            {
                return Forbid();
            }

            organizationToUpdate.Name = model.Name;
            _context.Update(organizationToUpdate);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}