using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register(string? type = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            var model = new RegisterViewModel { AccountType = type ?? "Applicant" };
            return View(model);
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.AccountType == "Employer" && string.IsNullOrEmpty(model.CompanyName))
            {
                ModelState.AddModelError("CompanyName", "اسم الشركة مطلوب للمسجلين كأصحاب عمل");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true // Auto-confirm for demo
            };

            if (model.AccountType == "Employer")
            {
                user.CompanyName = model.CompanyName;
                user.Industry = model.Industry;
            }

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var role = model.AccountType == "Employer" ? "Employer" : "Applicant";
                await _userManager.AddToRoleAsync(user, role);
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["Success"] = $"مرحباً {user.FullName}! تم إنشاء حسابك بنجاح.";

                return role == "Employer"
                    ? RedirectToAction("Dashboard", "Employer")
                    : RedirectToAction("Dashboard", "Applicant");
            }

            foreach (var error in result.Errors)
            {
                string arabicError = error.Code switch
                {
                    "DuplicateUserName" or "DuplicateEmail" => "هذا البريد الإلكتروني مسجل مسبقاً",
                    "PasswordTooShort" => "كلمة المرور قصيرة جداً، يجب أن تكون 8 أحرف على الأقل",
                    "PasswordRequiresNonAlphanumeric" => "كلمة المرور يجب أن تحتوي على رمز خاص",
                    "PasswordRequiresDigit" => "كلمة المرور يجب أن تحتوي على رقم",
                    "PasswordRequiresUpper" => "كلمة المرور يجب أن تحتوي على حرف كبير",
                    _ => error.Description
                };
                ModelState.AddModelError(string.Empty, arabicError);
            }

            return View(model);
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.IsActive)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "تم تعطيل هذا الحساب. يرجى التواصل مع الإدارة.");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                // Redirect based on role
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                        return RedirectToAction("Dashboard", "Admin", new { area = "" });
                    if (roles.Contains("Employer"))
                        return RedirectToAction("Dashboard", "Employer");
                    if (roles.Contains("Applicant"))
                        return RedirectToAction("Dashboard", "Applicant");
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "الحساب محظور مؤقتاً بسبب محاولات خاطئة متعددة. حاول مجدداً بعد 5 دقائق.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
            }

            return View(model);
        }

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "تم تسجيل الخروج بنجاح";
            return RedirectToAction("Index", "Home");
        }

        // GET: Change Password
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View();

        // POST: Change Password
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "كلمتا المرور الجديدتان غير متطابقتين");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "تم تغيير كلمة المرور بنجاح";
                return RedirectToAction("Profile", "Applicant");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View();
        }

        // Access Denied
        public IActionResult AccessDenied() => View();
    }
}
