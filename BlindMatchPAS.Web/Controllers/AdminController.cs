using BlindMatchPAS.Web.Services.Interfaces;
using BlindMatchPAS.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Web.Controllers;

[Authorize(Roles = RoleNames.AdminRoles)]
[Route("admin")]
public class AdminController(IAdminService adminService) : Controller
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() => View(await adminService.GetDashboardAsync());

    [HttpGet("users")]
    public async Task<IActionResult> Users() => View(await adminService.GetUsersAsync());

    [HttpPost("users")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var page = await adminService.GetUsersAsync();
            page.NewUser = model;
            return View("Users", page);
        }

        var result = await adminService.CreateUserAsync(model);
        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("users/{id}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(string id)
    {
        var result = await adminService.ToggleUserAsync(id);
        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet("research-areas")]
    public async Task<IActionResult> ResearchAreas() => View(await adminService.GetResearchAreasAsync());

    [HttpPost("research-areas")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResearchArea(ResearchAreaEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var page = await adminService.GetResearchAreasAsync();
            page.NewResearchArea = model;
            return View("ResearchAreas", page);
        }

        var result = await adminService.SaveResearchAreaAsync(model);
        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";
        return RedirectToAction(nameof(ResearchAreas));
    }

    [HttpPost("research-areas/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleResearchArea(int id)
    {
        var result = await adminService.ToggleResearchAreaAsync(id);
        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";
        return RedirectToAction(nameof(ResearchAreas));
    }

    [HttpGet("proposals")]
    public async Task<IActionResult> Proposals(string? searchTerm, int? researchAreaId, ProposalStatus? status) =>
        View(await adminService.GetProposalOversightAsync(searchTerm, researchAreaId, status));

    [HttpGet("matches")]
    public async Task<IActionResult> Matches() => View(await adminService.GetMatchOversightAsync());

    [HttpGet("matches/{proposalId:int}/reassign")]
    public async Task<IActionResult> ReassignProposal(int proposalId)
    {
        var model = await adminService.BuildReassignmentViewModelAsync(proposalId);
        return model is null ? NotFound() : View("Reassign", model);
    }

    [HttpPost("matches/reassign")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReassignment(AdminReassignmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var rebuilt = await adminService.BuildReassignmentViewModelAsync(model.ProposalId);
            if (rebuilt is null)
            {
                return NotFound();
            }

            rebuilt.SupervisorId = model.SupervisorId;
            rebuilt.Reason = model.Reason;
            return View(rebuilt);
        }

        var result = await adminService.ReassignAsync(model, User.GetUserId());
        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";
        return RedirectToAction(nameof(Matches));
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs() => View(await adminService.GetAuditLogsAsync());
}
