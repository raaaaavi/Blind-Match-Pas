using BlindMatchPAS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlindMatchPAS.Web.Controllers;

[AllowAnonymous]
public class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        await signInManager.SignInWithClaimsAsync(user, model.RememberMe, [new Claim("DisplayName", user.DisplayName)]);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return await RedirectToRoleDashboardAsync(user);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await userManager.Users.AnyAsync(x => x.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName,
            RegistrationNumber = model.RegistrationNumber,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await userManager.AddToRoleAsync(user, RoleNames.Student);
        context.StudentProfiles.Add(new StudentProfile
        {
            UserId = user.Id,
            StudentIdentifier = model.RegistrationNumber,
            Programme = model.Programme,
            GroupName = model.GroupName,
            TeamMemberNames = model.TeamMemberNames
        });
        await context.SaveChangesAsync();

        await signInManager.SignInWithClaimsAsync(user, false, [new Claim("DisplayName", user.DisplayName)]);
        TempData["StatusMessage"] = "Welcome to Blind-Match PAS. Your student account is ready.";
        TempData["StatusType"] = "success";
        return RedirectToAction("Dashboard", "Student");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task<IActionResult> RedirectToRoleDashboardAsync(ApplicationUser user)
    {
        if (await userManager.IsInRoleAsync(user, RoleNames.SystemAdmin) || await userManager.IsInRoleAsync(user, RoleNames.ModuleLeader))
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        if (await userManager.IsInRoleAsync(user, RoleNames.Supervisor))
        {
            return RedirectToAction("Dashboard", "Supervisor");
        }

        return RedirectToAction("Dashboard", "Student");
    }
}
