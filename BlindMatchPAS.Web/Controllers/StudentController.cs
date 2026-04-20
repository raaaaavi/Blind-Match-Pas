using BlindMatchPAS.Web.Services.Interfaces;
using BlindMatchPAS.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Web.Controllers;

/// <summary>
/// StudentController - Handles all student-related requests
/// </summary>
[Authorize(Roles = RoleNames.Student)]
[Route("student")]
public class StudentController(IProposalService proposalService) : Controller
{
    // ===== DASHBOARD AND LISTING ACTIONS =====

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var model = await proposalService.GetStudentDashboardAsync(User.GetUserId());
        return View(model);
    }

    [HttpGet("proposals")]
    public async Task<IActionResult> Proposals()
    {
        var proposals = await proposalService.GetStudentProposalsAsync(User.GetUserId());
        return View(proposals);
    }

    // ===== PROPOSAL CREATION =====

    [HttpGet("proposals/create")]
    public async Task<IActionResult> CreateProposal()
    {
        var model = await proposalService.BuildProposalFormAsync(User.GetUserId());
        return View("ProposalForm", model);
    }

    [HttpPost("proposals/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProposal(ProposalFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ResearchAreas = (await proposalService.BuildProposalFormAsync(User.GetUserId())).ResearchAreas;
            return View("ProposalForm", model);
        }

        var result = await proposalService.CreateProposalAsync(User.GetUserId(), model);

        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";

        if (!result.Success)
        {
            model.ResearchAreas = (await proposalService.BuildProposalFormAsync(User.GetUserId())).ResearchAreas;
            return View("ProposalForm", model);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data });
    }

    // ===== PROPOSAL EDITING =====

    [HttpGet("proposals/{id:int}/edit")]
    public async Task<IActionResult> EditProposal(int id)
    {
        // ✅ Added validation
        if (id <= 0)
        {
            return BadRequest("Invalid proposal ID");
        }

        var model = await proposalService.BuildProposalFormAsync(User.GetUserId(), id);
        return model.Id is null ? NotFound() : View("ProposalForm", model);
    }

    [HttpPost("proposals/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProposal(int id, ProposalFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ResearchAreas = (await proposalService.BuildProposalFormAsync(User.GetUserId(), id)).ResearchAreas;
            return View("ProposalForm", model);
        }

        var result = await proposalService.UpdateProposalAsync(User.GetUserId(), id, model);

        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";

        return RedirectToAction(nameof(Details), new { id });
    }

    // ===== PROPOSAL DETAILS =====

    [HttpGet("proposals/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        // ✅ Added validation
        if (id <= 0)
        {
            return BadRequest("Invalid proposal ID");
        }

        var proposal = await proposalService.GetStudentProposalDetailsAsync(User.GetUserId(), id);
        return proposal is null ? NotFound() : View(proposal);
    }

    // ===== WITHDRAW =====

    [HttpPost("proposals/{id:int}/withdraw")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        // ✅ Added validation
        if (id <= 0)
        {
            return BadRequest("Invalid proposal ID");
        }

        var result = await proposalService.WithdrawProposalAsync(User.GetUserId(), id);

        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";

        return RedirectToAction(nameof(Proposals));
    }
}