using BlindMatchPAS.Web.Services.Interfaces;
using BlindMatchPAS.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Web.Controllers;

/// <summary>
/// StudentController - Handles all student-related requests
/// 
/// Routes: /student/*
/// Authorization: Requires [Student] role
/// 
/// Responsibilities:
/// - Student dashboard and proposal listing
/// - Proposal submission, editing, and withdrawal
/// - View proposal details and status
/// - Track proposal lifecycle and matches
/// 
/// Key Features:
/// - Anti-forgery token validation on POST requests
/// - User identity extraction via User.GetUserId()
/// - Business logic delegation to IProposalService
/// </summary>
[Authorize(Roles = RoleNames.Student)]
[Route("student")]
public class StudentController(IProposalService proposalService) : Controller
{
    // ===== DASHBOARD AND LISTING ACTIONS =====
    
    /// <summary>
    /// GET: /student/dashboard
    /// Displays student dashboard with proposal summary and statistics
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var model = await proposalService.GetStudentDashboardAsync(User.GetUserId());
        return View(model);
    }

    /// <summary>
    /// GET: /student/proposals
    /// Lists all proposals owned by the current student
    /// Shows status, dates, and action buttons for each proposal
    /// </summary>
    [HttpGet("proposals")]
    public async Task<IActionResult> Proposals()
    {
        var proposals = await proposalService.GetStudentProposalsAsync(User.GetUserId());
        return View(proposals);
    }

    // ===== PROPOSAL CREATION ACTIONS =====
    
    /// <summary>
    /// GET: /student/proposals/create
    /// Displays form for creating a new proposal
    /// Loads available research areas for dropdown
    /// </summary>
    [HttpGet("proposals/create")]
    public async Task<IActionResult> CreateProposal()
    {
        var model = await proposalService.BuildProposalFormAsync(User.GetUserId());
        return View("ProposalForm", model);
    }

    /// <summary>
    /// POST: /student/proposals/create
    /// Submits a new proposal for review
    /// Validates model state, calls service to create proposal
    /// Redirects to proposal details on success
    /// </summary>
    [HttpPost("proposals/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProposal(ProposalFormViewModel model)
    {
        // Client-side validation failed - redisplay form with research areas
        if (!ModelState.IsValid)
        {
            model.ResearchAreas = (await proposalService.BuildProposalFormAsync(User.GetUserId())).ResearchAreas;
            return View("ProposalForm", model);
        }

        // Attempt to create proposal through service layer
        var result = await proposalService.CreateProposalAsync(User.GetUserId(), model);
        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";
        
        if (!result.Success)
        {
            // Server-side validation failed - redisplay form with error
            model.ResearchAreas = (await proposalService.BuildProposalFormAsync(User.GetUserId())).ResearchAreas;
            return View("ProposalForm", model);
        }

        // Success - redirect to newly created proposal details page
        return RedirectToAction(nameof(Details), new { id = result.Data });
    }

    // ===== PROPOSAL EDITING ACTIONS =====
    
    /// <summary>
    /// GET: /student/proposals/{id}/edit
    /// Loads proposal for editing
    /// Only allows editing if proposal is NOT matched/withdrawn
    /// </summary>
    [HttpGet("proposals/{id:int}/edit")]
    public async Task<IActionResult> EditProposal(int id)
    {
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

    [HttpGet("proposals/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var proposal = await proposalService.GetStudentProposalDetailsAsync(User.GetUserId(), id);
        return proposal is null ? NotFound() : View(proposal);
    }

    [HttpPost("proposals/{id:int}/withdraw")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        var result = await proposalService.WithdrawProposalAsync(User.GetUserId(), id);
        TempData["StatusMessage"] = result.Message;
        TempData["StatusType"] = result.Success ? "success" : "danger";
        return RedirectToAction(nameof(Proposals));
    }
}
