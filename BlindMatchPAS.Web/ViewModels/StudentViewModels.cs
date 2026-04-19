using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.Web.ViewModels.Student;

public class ProposalFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(160, MinimumLength = 8)]
    [Display(Name = "Project Title")]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 80)]
    public string Abstract { get; set; } = string.Empty;

    [Required, StringLength(500, MinimumLength = 3)]
    [Display(Name = "Technical Stack")]
    public string TechnicalStack { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Research Area")]
    public int ResearchAreaId { get; set; }

    [Required, StringLength(1500, MinimumLength = 40)]
    [Display(Name = "Proposed Methodology")]
    public string Methodology { get; set; } = string.Empty;

    [Required, StringLength(120)]
    [Display(Name = "Team Size / Group Info")]
    public string TeamSize { get; set; } = string.Empty;

    [StringLength(300)]
    [Display(Name = "Keywords / Tags")]
    public string Keywords { get; set; } = string.Empty;

    public List<SelectListItem> ResearchAreas { get; set; } = [];
}

public class ProposalListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ResearchArea { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool CanEdit { get; set; }
    public bool CanWithdraw { get; set; }
}

public class ProposalDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string TechnicalStack { get; set; } = string.Empty;
    public string ResearchArea { get; set; } = string.Empty;
    public string Methodology { get; set; } = string.Empty;
    public string TeamSize { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = [];
    public ProposalStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool CanEdit { get; set; }
    public bool CanWithdraw { get; set; }
    public string? SupervisorName { get; set; }
    public string? SupervisorEmail { get; set; }
    public string? SupervisorDepartment { get; set; }
    public string? SupervisorSpecialization { get; set; }
    public List<StatusTimelineItemViewModel> Timeline { get; set; } = [];
}

public class StudentDashboardViewModel
{
    public int TotalProposals { get; set; }
    public int PendingCount { get; set; }
    public int MatchedCount { get; set; }
    public int DraftCount { get; set; }
    public List<ProposalListItemViewModel> RecentProposals { get; set; } = [];
}
