using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Repositories.Interfaces;
using BlindMatchPAS.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Services;

public class MatchingService(
    ApplicationDbContext context,
    IProposalRepository proposalRepository,
    IResearchAreaRepository researchAreaRepository,
    IAuditService auditService) : IMatchingService
{
    public async Task<SupervisorDashboardViewModel> GetSupervisorDashboardAsync(string supervisorUserId)
    {
        var available = await proposalRepository.GetAnonymousAvailableForSupervisorAsync(null, null);
        var interested = await GetInterestedAsync(supervisorUserId);
        var matches = await GetConfirmedMatchesAsync(supervisorUserId);
        var expertiseCount = await context.SupervisorExpertise.CountAsync(x => x.SupervisorId == supervisorUserId);

        return new SupervisorDashboardViewModel
        {
            AvailableProposals = available.Count,
            ExpressedInterestCount = interested.Count,
            ConfirmedMatchCount = matches.Count,
            ExpertiseAreaCount = expertiseCount,
            SpotlightProposals = available.Take(3).Select(x => MapCard(x, x.SupervisorInterests.Any(i => i.SupervisorId == supervisorUserId))).ToList()
        };
    }

    public async Task<AnonymousProposalBrowserViewModel> GetAnonymousBrowserAsync(int? researchAreaId, string? searchTerm)
    {
        var proposals = await proposalRepository.GetAnonymousAvailableForSupervisorAsync(researchAreaId, searchTerm);
        var areas = await researchAreaRepository.GetActiveAsync();

        return new AnonymousProposalBrowserViewModel
        {
            ResearchAreaId = researchAreaId,
            SearchTerm = searchTerm ?? string.Empty,
            ResearchAreas = areas.Select(x => new ResearchAreaOptionViewModel { Id = x.Id, Name = x.Name, AccentColor = x.AccentColor }).ToList(),
            Proposals = proposals.Select(x => MapCard(x, false)).ToList()
        };
    }

    public async Task<SupervisorProposalDetailsViewModel?> GetSupervisorProposalDetailsAsync(string supervisorUserId, int proposalId)
    {
        var proposal = await proposalRepository.GetByIdAsync(proposalId);
        if (proposal is null)
        {
            return null;
        }

        var isMatchedWithCurrentSupervisor = proposal.Match?.SupervisorId == supervisorUserId;
        var hasInterest = proposal.SupervisorInterests.Any(x => x.SupervisorId == supervisorUserId);

        return new SupervisorProposalDetailsViewModel
        {
            Id = proposal.Id,
            Title = proposal.Title,
            Abstract = proposal.Abstract,
            TechnicalStack = proposal.TechnicalStack,
            ResearchArea = proposal.ResearchArea?.Name ?? string.Empty,
            Methodology = proposal.Methodology,
            Keywords = proposal.Keywords.Select(x => x.Keyword).ToList(),
            Status = proposal.Status,
            SubmittedAt = proposal.SubmittedAt,
            HasExpressedInterest = hasInterest,
            CanConfirmMatch = !proposal.IsMatched && !proposal.IsWithdrawn,
            CanRevealIdentity = isMatchedWithCurrentSupervisor,
            StudentName = isMatchedWithCurrentSupervisor ? proposal.StudentOwner?.DisplayName : null,
            StudentEmail = isMatchedWithCurrentSupervisor ? proposal.StudentOwner?.Email : null,
            GroupName = isMatchedWithCurrentSupervisor ? proposal.StudentOwner?.StudentProfile?.GroupName : null,
            TeamMembers = isMatchedWithCurrentSupervisor ? proposal.StudentOwner?.StudentProfile?.TeamMemberNames : null,
            Timeline = proposal.StatusHistory.OrderBy(x => x.ChangedAt).Select(x => new StatusTimelineItemViewModel
            {
                Status = x.Status.ToString(),
                Note = x.Note,
                ChangedAt = x.ChangedAt
            }).ToList()
        };
    }

    public async Task<List<SupervisorProposalCardViewModel>> GetInterestedAsync(string supervisorUserId)
    {
        var proposals = await proposalRepository.GetInterestedProposalsAsync(supervisorUserId);
        return proposals.Select(x => MapCard(x, x.Match?.SupervisorId == supervisorUserId)).ToList();
    }

    public async Task<List<SupervisorProposalCardViewModel>> GetConfirmedMatchesAsync(string supervisorUserId)
    {
        var proposals = await proposalRepository.GetMatchedProposalsAsync(supervisorUserId);
        return proposals.Select(x => MapCard(x, true)).ToList();
    }

    public async Task<ExpertiseManagementViewModel> BuildExpertiseViewModelAsync(string supervisorUserId)
    {
        var areas = await researchAreaRepository.GetActiveAsync();
        var selected = await context.SupervisorExpertise
            .Where(x => x.SupervisorId == supervisorUserId)
            .Select(x => x.ResearchAreaId)
            .ToListAsync();

        return new ExpertiseManagementViewModel
        {
            SelectedResearchAreaIds = selected,
            ResearchAreas = areas.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList()
        };
    }

    public async Task<ServiceResult> UpdateExpertiseAsync(string supervisorUserId, ExpertiseManagementViewModel model)
    {
        var existing = context.SupervisorExpertise.Where(x => x.SupervisorId == supervisorUserId);
        context.SupervisorExpertise.RemoveRange(existing);

        foreach (var researchAreaId in model.SelectedResearchAreaIds.Distinct())
        {
            context.SupervisorExpertise.Add(new SupervisorExpertise { SupervisorId = supervisorUserId, ResearchAreaId = researchAreaId });
        }

        await context.SaveChangesAsync();
        await auditService.RecordAsync("Expertise updated", nameof(SupervisorExpertise), supervisorUserId, supervisorUserId, "Supervisor updated preferred research areas.");
        return ServiceResult.Ok("Expertise preferences saved.");
    }

    public async Task<ServiceResult> ExpressInterestAsync(string supervisorUserId, int proposalId)
    {
        var proposal = await proposalRepository.GetByIdAsync(proposalId);
        if (proposal is null || proposal.IsMatched || proposal.IsWithdrawn)
        {
            return ServiceResult.Fail("This proposal is no longer available for interest.");
        }

        if (proposal.SupervisorInterests.Any(x => x.SupervisorId == supervisorUserId))
        {
            return ServiceResult.Fail("You have already expressed interest in this proposal.");
        }

        proposal.SupervisorInterests.Add(new SupervisorInterest
        {
            ProposalId = proposalId,
            SupervisorId = supervisorUserId,
            CreatedAt = DateTime.UtcNow
        });

        if (proposal.Status == ProposalStatus.PendingReview || proposal.Status == ProposalStatus.Submitted)
        {
            proposal.Status = ProposalStatus.UnderReview;
            proposal.StatusHistory.Add(new ProposalStatusHistory
            {
                Status = ProposalStatus.UnderReview,
                Note = "Supervisor expressed interest in blind review stage.",
                ChangedByUserId = supervisorUserId,
                ChangedAt = DateTime.UtcNow
            });
        }

        proposal.UpdatedAt = DateTime.UtcNow;
        proposalRepository.Update(proposal);
        await proposalRepository.SaveChangesAsync();
        await auditService.RecordAsync("Interest expressed", nameof(Proposal), proposal.Id.ToString(), supervisorUserId, $"Interest expressed for proposal '{proposal.Title}'.");
        return ServiceResult.Ok("Interest recorded successfully.");
    }

    public async Task<ServiceResult> ConfirmMatchAsync(string supervisorUserId, int proposalId)
    {
        var proposal = await proposalRepository.GetByIdAsync(proposalId);
        if (proposal is null)
        {
            return ServiceResult.Fail("Proposal not found.");
        }

        if (proposal.IsMatched || proposal.Match is not null)
        {
            return ServiceResult.Fail("This proposal already has a confirmed match.");
        }

        if (proposal.IsWithdrawn)
        {
            return ServiceResult.Fail("Withdrawn proposals cannot be matched.");
        }

        var interest = proposal.SupervisorInterests.FirstOrDefault(x => x.SupervisorId == supervisorUserId);
        if (interest is null)
        {
            proposal.SupervisorInterests.Add(new SupervisorInterest
            {
                ProposalId = proposal.Id,
                SupervisorId = supervisorUserId,
                CreatedAt = DateTime.UtcNow,
                IsConfirmed = true
            });
        }
        else
        {
            interest.IsConfirmed = true;
        }

        proposal.IsMatched = true;
        proposal.Status = ProposalStatus.Matched;
        proposal.UpdatedAt = DateTime.UtcNow;
        proposal.Match = new Match
        {
            ProposalId = proposal.Id,
            SupervisorId = supervisorUserId,
            ConfirmedAt = DateTime.UtcNow,
            RevealedAt = DateTime.UtcNow,
            Status = MatchStatus.Confirmed,
            CreatedByAdminOverride = false
        };

        proposal.StatusHistory.Add(new ProposalStatusHistory
        {
            Status = ProposalStatus.Matched,
            Note = "Supervisor confirmed match. Identities revealed to both parties.",
            ChangedByUserId = supervisorUserId,
            ChangedAt = DateTime.UtcNow
        });

        proposalRepository.Update(proposal);
        await proposalRepository.SaveChangesAsync();
        await auditService.RecordAsync("Match confirmed", nameof(Proposal), proposal.Id.ToString(), supervisorUserId, $"Supervisor confirmed match for proposal '{proposal.Title}'.");
        return ServiceResult.Ok("Match confirmed successfully.");
    }

    private static SupervisorProposalCardViewModel MapCard(Proposal proposal, bool revealIdentity) => new()
    {
        Id = proposal.Id,
        Title = proposal.Title,
        Abstract = proposal.Abstract,
        TechnicalStack = proposal.TechnicalStack,
        ResearchArea = proposal.ResearchArea?.Name ?? string.Empty,
        Keywords = proposal.Keywords.Select(x => x.Keyword).ToList(),
        Status = proposal.Status,
        SubmittedAt = proposal.SubmittedAt,
        HasExpressedInterest = proposal.SupervisorInterests.Any(),
        IsMatchedWithCurrentSupervisor = revealIdentity,
        RevealedStudentName = revealIdentity ? proposal.StudentOwner?.DisplayName : null,
        RevealedStudentEmail = revealIdentity ? proposal.StudentOwner?.Email : null,
        RevealedGroupInfo = revealIdentity ? proposal.StudentOwner?.StudentProfile?.GroupName : null
    };
}
