using BlindMatchPAS.Web.Repositories.Interfaces;
using BlindMatchPAS.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.Web.Services;

public class ProposalService(
    IProposalRepository proposalRepository,
    IResearchAreaRepository researchAreaRepository,
    IAuditService auditService) : IProposalService
{
    public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentUserId)
    {
        var proposals = await proposalRepository.GetStudentProposalsAsync(studentUserId);
        return new StudentDashboardViewModel
        {
            TotalProposals = proposals.Count,
            PendingCount = proposals.Count(x => x.Status is ProposalStatus.Submitted or ProposalStatus.PendingReview or ProposalStatus.UnderReview),
            MatchedCount = proposals.Count(x => x.Status == ProposalStatus.Matched),
            DraftCount = proposals.Count(x => x.Status == ProposalStatus.Draft),
            RecentProposals = proposals.Take(4).Select(MapListItem).ToList()
        };
    }

    public async Task<List<ProposalListItemViewModel>> GetStudentProposalsAsync(string studentUserId)
    {
        var proposals = await proposalRepository.GetStudentProposalsAsync(studentUserId);
        return proposals.Select(MapListItem).ToList();
    }

    public async Task<ProposalFormViewModel> BuildProposalFormAsync(string studentUserId, int? proposalId = null)
    {
        var areas = await researchAreaRepository.GetActiveAsync();
        var viewModel = new ProposalFormViewModel
        {
            ResearchAreas = areas.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList()
        };

        if (!proposalId.HasValue)
        {
            return viewModel;
        }

        var proposal = await proposalRepository.GetByIdForStudentAsync(proposalId.Value, studentUserId);
        if (proposal is null)
        {
            return viewModel;
        }

        viewModel.Id = proposal.Id;
        viewModel.Title = proposal.Title;
        viewModel.Abstract = proposal.Abstract;
        viewModel.TechnicalStack = proposal.TechnicalStack;
        viewModel.ResearchAreaId = proposal.ResearchAreaId;
        viewModel.Methodology = proposal.Methodology;
        viewModel.TeamSize = proposal.TeamSize;
        viewModel.Keywords = string.Join(", ", proposal.Keywords.Select(x => x.Keyword));
        return viewModel;
    }

    public async Task<ServiceResult<int>> CreateProposalAsync(string studentUserId, ProposalFormViewModel model)
    {
        var area = await researchAreaRepository.GetByIdAsync(model.ResearchAreaId);
        if (area is null || !area.IsActive)
        {
            return ServiceResult<int>.Fail("Please select an active research area.");
        }

        var proposal = new Proposal
        {
            Title = model.Title.Trim(),
            Abstract = model.Abstract.Trim(),
            TechnicalStack = model.TechnicalStack.Trim(),
            ResearchAreaId = model.ResearchAreaId,
            Methodology = model.Methodology.Trim(),
            TeamSize = model.TeamSize.Trim(),
            StudentOwnerId = studentUserId,
            Status = ProposalStatus.PendingReview,
            SubmittedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var keyword in ParseKeywords(model.Keywords))
        {
            proposal.Keywords.Add(new ProposalKeyword { Keyword = keyword });
        }

        proposal.StatusHistory.Add(new ProposalStatusHistory
        {
            Status = ProposalStatus.Submitted,
            Note = "Proposal submitted by student/group lead.",
            ChangedByUserId = studentUserId,
            ChangedAt = DateTime.UtcNow
        });

        proposal.StatusHistory.Add(new ProposalStatusHistory
        {
            Status = ProposalStatus.PendingReview,
            Note = "Proposal entered blind review queue.",
            ChangedByUserId = studentUserId,
            ChangedAt = DateTime.UtcNow
        });

        await proposalRepository.AddAsync(proposal);
        await proposalRepository.SaveChangesAsync();
        await auditService.RecordAsync("Proposal created", nameof(Proposal), proposal.Id.ToString(), studentUserId, $"Proposal '{proposal.Title}' created.");

        return ServiceResult<int>.Ok(proposal.Id, "Proposal submitted successfully.");
    }

    public async Task<ServiceResult> UpdateProposalAsync(string studentUserId, int proposalId, ProposalFormViewModel model)
    {
        var proposal = await proposalRepository.GetByIdForStudentAsync(proposalId, studentUserId);
        if (proposal is null)
        {
            return ServiceResult.Fail("Proposal not found.");
        }

        if (proposal.IsMatched || proposal.IsWithdrawn || proposal.Status == ProposalStatus.Matched || proposal.Status == ProposalStatus.Withdrawn)
        {
            return ServiceResult.Fail("Matched or withdrawn proposals can no longer be edited.");
        }

        proposal.Title = model.Title.Trim();
        proposal.Abstract = model.Abstract.Trim();
        proposal.TechnicalStack = model.TechnicalStack.Trim();
        proposal.ResearchAreaId = model.ResearchAreaId;
        proposal.Methodology = model.Methodology.Trim();
        proposal.TeamSize = model.TeamSize.Trim();
        proposal.UpdatedAt = DateTime.UtcNow;

        proposal.Keywords.Clear();
        foreach (var keyword in ParseKeywords(model.Keywords))
        {
            proposal.Keywords.Add(new ProposalKeyword { ProposalId = proposal.Id, Keyword = keyword });
        }

        proposal.StatusHistory.Add(new ProposalStatusHistory
        {
            Status = proposal.Status,
            Note = "Proposal updated by owner.",
            ChangedByUserId = studentUserId,
            ChangedAt = DateTime.UtcNow
        });

        proposalRepository.Update(proposal);
        await proposalRepository.SaveChangesAsync();
        await auditService.RecordAsync("Proposal edited", nameof(Proposal), proposal.Id.ToString(), studentUserId, $"Proposal '{proposal.Title}' updated.");
        return ServiceResult.Ok("Proposal updated successfully.");
    }

    public async Task<ServiceResult> WithdrawProposalAsync(string studentUserId, int proposalId)
    {
        var proposal = await proposalRepository.GetByIdForStudentAsync(proposalId, studentUserId);
        if (proposal is null)
        {
            return ServiceResult.Fail("Proposal not found.");
        }

        if (proposal.IsMatched)
        {
            return ServiceResult.Fail("Matched proposals cannot be withdrawn.");
        }

        proposal.IsWithdrawn = true;
        proposal.Status = ProposalStatus.Withdrawn;
        proposal.UpdatedAt = DateTime.UtcNow;
        proposal.StatusHistory.Add(new ProposalStatusHistory
        {
            Status = ProposalStatus.Withdrawn,
            Note = "Proposal withdrawn by owner.",
            ChangedByUserId = studentUserId,
            ChangedAt = DateTime.UtcNow
        });

        proposalRepository.Update(proposal);
        await proposalRepository.SaveChangesAsync();
        await auditService.RecordAsync("Proposal withdrawn", nameof(Proposal), proposal.Id.ToString(), studentUserId, $"Proposal '{proposal.Title}' withdrawn.");
        return ServiceResult.Ok("Proposal withdrawn successfully.");
    }

    public async Task<ProposalDetailsViewModel?> GetStudentProposalDetailsAsync(string studentUserId, int proposalId)
    {
        var proposal = await proposalRepository.GetByIdForStudentAsync(proposalId, studentUserId);
        return proposal is null ? null : MapDetails(proposal);
    }

    private static ProposalListItemViewModel MapListItem(Proposal proposal) => new()
    {
        Id = proposal.Id,
        Title = proposal.Title,
        ResearchArea = proposal.ResearchArea?.Name ?? string.Empty,
        Status = proposal.Status,
        SubmittedAt = proposal.SubmittedAt,
        UpdatedAt = proposal.UpdatedAt,
        CanEdit = !proposal.IsMatched && !proposal.IsWithdrawn,
        CanWithdraw = !proposal.IsMatched && !proposal.IsWithdrawn
    };

    private static ProposalDetailsViewModel MapDetails(Proposal proposal) => new()
    {
        Id = proposal.Id,
        Title = proposal.Title,
        Abstract = proposal.Abstract,
        TechnicalStack = proposal.TechnicalStack,
        ResearchArea = proposal.ResearchArea?.Name ?? string.Empty,
        Methodology = proposal.Methodology,
        TeamSize = proposal.TeamSize,
        Keywords = proposal.Keywords.Select(x => x.Keyword).ToList(),
        Status = proposal.Status,
        SubmittedAt = proposal.SubmittedAt,
        CanEdit = !proposal.IsMatched && !proposal.IsWithdrawn,
        CanWithdraw = !proposal.IsMatched && !proposal.IsWithdrawn,
        SupervisorName = proposal.Match?.Supervisor?.DisplayName,
        SupervisorEmail = proposal.Match?.Supervisor?.Email,
        SupervisorDepartment = proposal.Match?.Supervisor?.SupervisorProfile?.Department,
        SupervisorSpecialization = proposal.Match?.Supervisor?.SupervisorProfile?.Specialization,
        Timeline = proposal.StatusHistory.OrderBy(x => x.ChangedAt).Select(x => new StatusTimelineItemViewModel
        {
            Status = x.Status.ToString(),
            Note = x.Note,
            ChangedAt = x.ChangedAt
        }).ToList()
    };

    private static IEnumerable<string> ParseKeywords(string? rawKeywords) => (rawKeywords ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(x => x.Trim())
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Take(8);
}
