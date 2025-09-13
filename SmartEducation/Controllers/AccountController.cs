using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartEducation.dbContext;
using SmartEducation.Entities;
using SmartEducation.Services;
using SmartEducation.ViewModels;

namespace SmartEducation.Controllers
{
    public class AccountController : Controller
    {
        private readonly SmartEduDbContext _context;
        private SignInManager<User> _signInManager;
        private UserManager<User> _userManager;
        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, SmartEduDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "An account with this email address already exists.");
                    return View(model);
                }

                string otpCode = Verify_Email_Services.GenerateRandomKey(6).ToUpper();
                string verificationId = Guid.NewGuid().ToString();

                var otpRecord = new OtpVerification
                {
                    Id = verificationId,
                    OtpCode = otpCode,
                    Email = model.Email,
                    SerializedRegistrationData = JsonConvert.SerializeObject(model),
                    ExpirationDate = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false
                };
                _context.OtpVerifications.Add(otpRecord);
                await _context.SaveChangesAsync();

                var verificationUrl = Url.Action("VerifyByEmail", "Account", new { verificationId = verificationId }, Request.Scheme);

                bool emailSent = await Verify_Email_Services.Send_OTP_CodeAsync(
                    otpCode,
                    verificationId,
                    model.Email,
                    $"{model.First_Name} {model.Last_Name}",
                    verificationUrl
                );

                if (emailSent)
                {
                    return RedirectToAction("VerifyOtp", new { email = model.Email });
                }
                else
                {
                    ModelState.AddModelError("", "We couldn't send a verification email. Please check your email address and try again.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }
            var model = new VerifyOtpViewModel { Email = email };
            return View(model);
        }

        private async Task<(bool IsSuccess, List<string> Errors)> CreateUserAccount(RegisterViewModel model)
        {
            var user = new User { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                user.FirstName = model.First_Name;
                user.LastName = model.Last_Name;
                user.DateCreated = DateTime.Now;
                user.DateUpdated = DateTime.Now;
                user.LastLogin = DateTime.Now;
                await _userManager.UpdateAsync(user);

                await _userManager.AddToRoleAsync(user, "User");
                return (true, new List<string>());
            }
            else
            {
                return (false, result.Errors.Select(e => e.Description).ToList());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var otpRecord = await _context.OtpVerifications
                    .FirstOrDefaultAsync(o => o.Email == model.Email && o.OtpCode == model.OtpCode.ToUpper() && !o.IsUsed);

                if (otpRecord == null)
                {
                    ModelState.AddModelError("OtpCode", "The verification code is invalid.");
                    return View(model);
                }

                if (otpRecord.ExpirationDate < DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "The verification code has expired. Please register again to get a new one.");
                    _context.OtpVerifications.Remove(otpRecord);
                    await _context.SaveChangesAsync();
                    return View(model);
                }

                var registrationModel = JsonConvert.DeserializeObject<RegisterViewModel>(otpRecord.SerializedRegistrationData);

                var (isSuccess, errors) = await CreateUserAccount(registrationModel);

                if (isSuccess)
                {
                    otpRecord.IsUsed = true;
                    await _context.SaveChangesAsync();
                    // Redirect to login with a success message
                    TempData["SuccessMessage"] = "Email verified successfully! Please log in.";
                    return RedirectToAction("LogIn", "Account");
                }
                else
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyByEmail(string verificationId)
        {
            if (string.IsNullOrEmpty(verificationId))
            {
                TempData["ErrorMessage"] = "Invalid verification link.";
                return RedirectToAction("LogIn");
            }

            var otpRecord = await _context.OtpVerifications.FirstOrDefaultAsync(o => o.Id == verificationId && !o.IsUsed);

            if (otpRecord == null)
            {
                TempData["ErrorMessage"] = "This verification link is invalid or has already been used.";
                return RedirectToAction("LogIn");
            }

            if (otpRecord.ExpirationDate < DateTime.UtcNow)
            {
                _context.OtpVerifications.Remove(otpRecord);
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = "This verification link has expired. Please register again.";
                return RedirectToAction("LogIn");
            }

            var registrationModel = JsonConvert.DeserializeObject<RegisterViewModel>(otpRecord.SerializedRegistrationData);
            var (isSuccess, errors) = await CreateUserAccount(registrationModel);

            if (isSuccess)
            {
                otpRecord.IsUsed = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Email verified successfully! Please log in.";
                return RedirectToAction("LogIn");
            }
            else
            {
                var errorString = string.Join(" ", errors);
                TempData["ErrorMessage"] = $"Account creation failed: {errorString}";
                return RedirectToAction("LogIn");
            }
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult LogIn(string returnURL = "")
        {
            var model = new LoginViewModel { ReturnUrl = returnURL };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password,
                            isPersistent: model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        // Get the user that just logged in
                        //var user = await _userManager.FindByNameAsync(model.Username);

                        //// Check their role and redirect accordingly
                        //if (await _userManager.IsInRoleAsync(user, "Admin"))
                        //{
                        //    return RedirectToAction("Index", "Admin");
                        //}
                        //else if (await _userManager.IsInRoleAsync(user, "OrganizationAdmin"))
                        //{
                        //    return RedirectToAction("Index", "Organization");
                        //}
                        //else
                        //{
                        //    // Default redirect for "User" role or any other roles
                        //    return RedirectToAction("Index", "Home");
                        //}
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ModelState.AddModelError("", "Invalid username/password.");
            return View(model);
        }

        [HttpGet]
        public ViewResult AccessDenied()
        {
            return View();
        }
    }
}
