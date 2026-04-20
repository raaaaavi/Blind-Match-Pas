using BlindMatchPAS.Web.Services.Interfaces;
using BlindMatchPAS.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Web.Controllers;

[Authorize(Roles = RoleNames.Supervisor)]
[Route("supervisor")]
public class SupervisorController(IMatchingService matchingService) : Controller
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var model = await matchingService.GetSupervisorDashboardAsync(User.GetUserId());
        return View(model);
    }

    [HttpGet("browse")]
    public async Task<IActionResult> Browse(int? researchAreaId, string? searchTerm)
    {
        var model = await matchingService.GetAnonymousBrowserAsync(researchAreaId, searchTerm);
        return View(model);
    }

    [HttpGet("proposals/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        // ✅ Validation added
        if (id <= 0)
        {
            return BadRequest("Invalid proposal ID");
        }

        var model = await matchingService.GetSupervisorProposalDetailsAsync(User.GetUserId(), id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost("proposals/{id:int}/interest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExpressInterest(int id)
    {
        // ✅ Validation added
        if (id <= 0)
        {
            return BadRequest("Invalid proposal ID");
        }

        var result = await matchingService.ExpressInterestAsync(User.GetUserId(), id);

        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("proposals/{id:int}/confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmMatch(int id)
    {
        // ✅ Validation added
        if (id <= 0)
        {
            return BadRequest("Invalid proposal ID");
        }

        var result = await matchingService.ConfirmMatchAsync(User.GetUserId(), id);

        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("interested")]
    public async Task<IActionResult> Interested()
    {
        var model = await matchingService.GetInterestedAsync(User.GetUserId());
        return View(model);
    }

    [HttpGet("matches")]
    public async Task<IActionResult> Matches()
    {
        var model = await matchingService.GetConfirmedMatchesAsync(User.GetUserId());
        return View(model);
    }

    [HttpGet("expertise")]
    public async Task<IActionResult> Expertise()
    {
        var model = await matchingService.BuildExpertiseViewModelAsync(User.GetUserId());
        return View(model);
    }

    [HttpPost("expertise")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Expertise(ExpertiseManagementViewModel model)
    {
        // ✅ Model validation added
        if (!ModelState.IsValid)
        {
            var rebuilt = await matchingService.BuildExpertiseViewModelAsync(User.GetUserId());
            return View(rebuilt);
        }

        var result = await matchingService.UpdateExpertiseAsync(User.GetUserId(), model);

        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";

        return RedirectToAction(nameof(Expertise));
    }
}