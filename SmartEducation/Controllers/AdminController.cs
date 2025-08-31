using DotNetEnv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.Entities;
using SmartEducation.ViewModels;
using System.Text.Json;

namespace SmartEducation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SmartEduDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(SmartEduDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index() => View();

        // ORGANIZATION MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> Organizations()
        {
            var organizations = await _context.Organizations.ToListAsync();
            return View(organizations);
        }

        [HttpGet]
        public IActionResult CreateOrganization()
        {
            return View(new CreateOrganizationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrganization(CreateOrganizationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var organization = new Organization { Name = model.Name };
                _context.Add(organization);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Organizations));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditOrganization(int? id)
        {
            if (id == null) return NotFound();
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null) return NotFound();

            var model = new EditOrganizationViewModel
            {
                Id = organization.Id,
                Name = organization.Name
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrganization(int id, EditOrganizationViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var organizationToUpdate = await _context.Organizations.FindAsync(id);
                if (organizationToUpdate == null) return NotFound();

                organizationToUpdate.Name = model.Name;

                try
                {
                    _context.Update(organizationToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Organizations.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Organizations));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteOrganization(int? id)
        {
            if (id == null) return NotFound();
            var organization = await _context.Organizations.FirstOrDefaultAsync(m => m.Id == id);
            if (organization == null) return NotFound();
            return View(organization);
        }

        [HttpPost, ActionName("DeleteOrganization")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrganizationConfirmed(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization != null)
            {
                _context.Organizations.Remove(organization);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Organizations));
        }

        [HttpGet]
        public async Task<IActionResult> ManageOrganizationUsers(int id)
        {
            var organization = await _context.Organizations
                                             .Include(o => o.Users)
                                             .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null) return NotFound();

            var usersNotInOrg = await _userManager.Users
                                                  .Where(u => u.OrganizationId != id || u.OrganizationId == null)
                                                  .ToListAsync();

            var model = new ManageOrganizationUsersViewModel
            {
                OrganizationId = organization.Id,
                OrganizationName = organization.Name,
                Members = organization.Users.ToList(),
                UsersNotInOrg = new SelectList(usersNotInOrg, "Id", "Email")
            };

            foreach (var member in model.Members)
            {
                if (await _userManager.IsInRoleAsync(member, "OrganizationAdmin"))
                {
                    model.OrganizationAdminId = member.Id;
                    model.OrganizationAdminEmail = member.Email;
                    break;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignOrgAdminRole(int organizationId, string userId)
        {
            var userToMakeAdmin = await _userManager.FindByIdAsync(userId);
            if (userToMakeAdmin == null || userToMakeAdmin.OrganizationId != organizationId)
            {
                return NotFound();
            }

            // Find and remove the current admin for this organization, if one exists
            var members = await _context.Users.Where(u => u.OrganizationId == organizationId).ToListAsync();
            foreach (var member in members)
            {
                if (await _userManager.IsInRoleAsync(member, "OrganizationAdmin"))
                {
                    await _userManager.RemoveFromRoleAsync(member, "OrganizationAdmin");
                    break;
                }
            }

            // Add the new user to the OrganizationAdmin role
            await _userManager.AddToRoleAsync(userToMakeAdmin, "OrganizationAdmin");

            return RedirectToAction("ManageOrganizationUsers", new { id = organizationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserInOrganization(ManageOrganizationUsersViewModel model)
        {
            if (!string.IsNullOrEmpty(model.NewUserEmail) && !string.IsNullOrEmpty(model.NewUserPassword))
            {
                var user = new User
                {
                    UserName = model.NewUserEmail,
                    Email = model.NewUserEmail,
                    OrganizationId = model.OrganizationId // Assign to the current organization
                };
                var result = await _userManager.CreateAsync(user, model.NewUserPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    return RedirectToAction("ManageOrganizationUsers", new { id = model.OrganizationId });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return await ManageOrganizationUsers(model.OrganizationId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToOrganization(int organizationId, string userIdToAdd)
        {
            if (userIdToAdd != null)
            {
                var user = await _userManager.FindByIdAsync(userIdToAdd);
                var organization = await _context.Organizations.FindAsync(organizationId);

                if (user != null && organization != null)
                {
                    user.OrganizationId = organizationId;
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction("ManageOrganizationUsers", new { id = organizationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromOrganization(int organizationId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.OrganizationId == organizationId)
            {
                user.OrganizationId = null; // Set organization to null
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("ManageOrganizationUsers", new { id = organizationId });
        }

        // USER MANAGEMENT
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.Include(u => u.Organization).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            var model = new UserViewModel
            {
                Organizations = new SelectList(_context.Organizations, "Id", "Name"),
                Roles = new SelectList(_roleManager.Roles, "Name", "Name")
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    OrganizationId = model.OrganizationId
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null)
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    return RedirectToAction(nameof(Users));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Repopulate lists if model state is invalid
            model.Organizations = new SelectList(_context.Organizations, "Id", "Name", model.OrganizationId);
            model.Roles = new SelectList(_roleManager.Roles, "Name", "Name", model.SelectedRoles);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                OrganizationId = user.OrganizationId,
                Organizations = new SelectList(_context.Organizations, "Id", "Name", user.OrganizationId),
                Roles = new SelectList(_roleManager.Roles, "Name", "Name"),
                SelectedRoles = userRoles.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.Email = model.Email;
                // Keep username in sync with email
                user.UserName = model.Email; 
                user.OrganizationId = model.OrganizationId;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update roles
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var selectedRoles = model.SelectedRoles ?? new List<string>();
                    await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
                    await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

                    // Optionally update password
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, model.Password);
                    }

                    return RedirectToAction(nameof(Users));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            // Repopulate lists if model state is invalid
            model.Organizations = new SelectList(_context.Organizations, "Id", "Name", model.OrganizationId);
            model.Roles = new SelectList(_roleManager.Roles, "Name", "Name", model.SelectedRoles);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }

        // SYSTEM CONFIGURATION
        [HttpGet]
        public IActionResult Configuration()
        {
            var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (!System.IO.File.Exists(envFilePath))
            {
                // Handle case where .env file is missing
                // You might want to create it with default values or show an error
                ModelState.AddModelError(string.Empty, ".env file not found in the project root.");
                return View(new ConfigurationViewModel());
            }

            Env.Load();

            var model = new ConfigurationViewModel
            {
                SENDER_EMAIL = Environment.GetEnvironmentVariable("SENDER_EMAIL"),
                SMTP_PASSWORD = Environment.GetEnvironmentVariable("SMTP_PASSWORD"),
                OPENAI_API_KEY = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
                OPEN_AI_MODEL = Environment.GetEnvironmentVariable("OPEN_AI_MODEL")
            };

            return View(model);
        }

        private void UpdateOrAddEnvVar(List<string> lines, string key, string value)
        {
            // Use a null-coalescing operator to handle null values gracefully
            value ??= "";

            int index = lines.FindIndex(line => line.TrimStart().StartsWith($"{key}="));
            string newEntry = $"{key}={value}";

            if (index != -1)
            {
                lines[index] = newEntry;
            }
            else
            {
                lines.Add(newEntry);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Configuration(ConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                if (!System.IO.File.Exists(envFilePath))
                {
                    // Create the file if it doesn't exist
                    System.IO.File.WriteAllText(envFilePath, string.Empty);
                }

                // Read all lines, then update or add the required keys
                var lines = System.IO.File.ReadAllLines(envFilePath).ToList();

                UpdateOrAddEnvVar(lines, "SENDER_EMAIL", model.SENDER_EMAIL);
                UpdateOrAddEnvVar(lines, "SMTP_PASSWORD", model.SMTP_PASSWORD);
                UpdateOrAddEnvVar(lines, "OPENAI_API_KEY", model.OPENAI_API_KEY);
                UpdateOrAddEnvVar(lines, "OPEN_AI_MODEL", model.OPEN_AI_MODEL);

                System.IO.File.WriteAllLines(envFilePath, lines);

                // Set a success message
                TempData["SuccessMessage"] = "Configuration saved successfully! Note: A server restart may be required for changes to take effect everywhere.";
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError(string.Empty, $"An error occurred while saving the configuration: {ex.Message}");
                return View(model);
            }

            return RedirectToAction(nameof(Configuration));
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            // Key Metrics
            var totalUsers = await _userManager.Users.CountAsync();
            var totalOrganizations = await _context.Organizations.CountAsync();
            var totalApiTokensUsed = await _context.ActivityRecommendations.SumAsync(ar => ar.TotalTokens);
            var totalNgssStandards = await _context.NGSS_Detailed_Standard.CountAsync();
            var totalGradeStandards = await _context.NGSS_Standard.CountAsync();
            var totalKids = await _context.Kids.CountAsync();
            var totalRecommendations = await _context.ActivityRecommendations.CountAsync();

            // Chart: User Role Distribution
            var roles = await _roleManager.Roles.ToListAsync();
            var userRoleData = new ChartData();
            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                userRoleData.Labels.Add(role.Name);
                userRoleData.Data.Add(usersInRole.Count);
            }

            // Chart: Recommendations in the Last 7 Days
            var recommendationsByDay = await _context.ActivityRecommendations
                .Where(r => r.DateTimeRequest >= sevenDaysAgo)
                .GroupBy(r => r.DateTimeRequest.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var recommendationsChartData = new ChartData();
            var dailyCounts = recommendationsByDay.ToDictionary(x => x.Date, x => x.Count);
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                recommendationsChartData.Labels.Add(date.ToString("MMM dd"));
                recommendationsChartData.Data.Add(dailyCounts.ContainsKey(date) ? dailyCounts[date] : 0);
            }

            // Chart: API Tokens Used in the Last 7 Days
            var tokensByDay = await _context.ActivityRecommendations
                .Where(r => r.DateTimeRequest >= sevenDaysAgo && r.TotalTokens.HasValue)
                .GroupBy(r => r.DateTimeRequest.Date)
                .Select(g => new { Date = g.Key, TotalTokens = g.Sum(r => r.TotalTokens.Value) })
                .ToListAsync();

            var tokensChartData = new ChartData();
            var dailyTokenCounts = tokensByDay.ToDictionary(x => x.Date, x => x.TotalTokens);
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                tokensChartData.Labels.Add(date.ToString("MMM dd"));
                tokensChartData.Data.Add(dailyTokenCounts.ContainsKey(date) ? dailyTokenCounts[date] : 0);
            }

            // Table: Organization User Counts
            var orgUserCounts = await _context.Organizations
                .Select(o => new OrganizationUserCount
                {
                    OrganizationName = o.Name,
                    UserCount = o.Users.Count()
                })
                .OrderByDescending(o => o.UserCount)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalOrganizations = totalOrganizations,
                TotalApiTokensUsed = totalApiTokensUsed,
                TotalNgssStandards = totalNgssStandards,
                TotalGradeStandards = totalGradeStandards,
                TotalKidsRegistered = totalKids,
                TotalRecommendations = totalRecommendations,
                UserRoleDistribution = userRoleData,
                RecommendationsLast7Days = recommendationsChartData,
                TokensUsedLast7Days = tokensChartData,
                OrganizationUserCounts = orgUserCounts
            };

            // Serialize chart data for JavaScript consumption
            ViewData["UserRoleDataJson"] = JsonSerializer.Serialize(model.UserRoleDistribution);
            ViewData["RecommendationsDataJson"] = JsonSerializer.Serialize(model.RecommendationsLast7Days);
            ViewData["TokensDataJson"] = JsonSerializer.Serialize(model.TokensUsedLast7Days);

            return View(model);
        }
    }
}