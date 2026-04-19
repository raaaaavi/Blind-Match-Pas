using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.Web.ViewModels.Supervisor;

public class SupervisorProposalCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string TechnicalStack { get; set; } = string.Empty;
    public string ResearchArea { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = [];
    public ProposalStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool HasExpressedInterest { get; set; }
    public bool IsMatchedWithCurrentSupervisor { get; set; }
    public string? RevealedStudentName { get; set; }
    public string? RevealedStudentEmail { get; set; }
    public string? RevealedGroupInfo { get; set; }
}

public class SupervisorProposalDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string TechnicalStack { get; set; } = string.Empty;
    public string ResearchArea { get; set; } = string.Empty;
    public string Methodology { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = [];
    public ProposalStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool HasExpressedInterest { get; set; }
    public bool CanConfirmMatch { get; set; }
    public bool CanRevealIdentity { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public string? GroupName { get; set; }
    public string? TeamMembers { get; set; }
    public List<StatusTimelineItemViewModel> Timeline { get; set; } = [];
}

public class AnonymousProposalBrowserViewModel
{
    public int? ResearchAreaId { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public List<ResearchAreaOptionViewModel> ResearchAreas { get; set; } = [];
    public List<SupervisorProposalCardViewModel> Proposals { get; set; } = [];
}

public class ExpertiseManagementViewModel
{
    [Display(Name = "Preferred Research Areas")]
    public List<int> SelectedResearchAreaIds { get; set; } = [];

    public List<SelectListItem> ResearchAreas { get; set; } = [];
}

public class SupervisorDashboardViewModel
{
    public int AvailableProposals { get; set; }
    public int ExpressedInterestCount { get; set; }
    public int ConfirmedMatchCount { get; set; }
    public int ExpertiseAreaCount { get; set; }
    public List<SupervisorProposalCardViewModel> SpotlightProposals { get; set; } = [];
}
