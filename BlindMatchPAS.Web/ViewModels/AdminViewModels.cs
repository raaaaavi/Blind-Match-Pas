using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.Web.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalStudents { get; set; }
    public int TotalSupervisors { get; set; }
    public int TotalProposals { get; set; }
    public int TotalMatchedProjects { get; set; }
    public int PendingReviews { get; set; }
    public List<AreaAnalyticsViewModel> ProposalsByArea { get; set; } = [];
    public List<AuditLogViewModel> RecentAuditLogs { get; set; } = [];
}

public class AreaAnalyticsViewModel
{
    public string AreaName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ResearchAreaManagementViewModel
{
    public List<ResearchAreaEditViewModel> ResearchAreas { get; set; } = [];
    public ResearchAreaEditViewModel NewResearchArea { get; set; } = new();
}

public class ResearchAreaEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(280)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string AccentColor { get; set; } = "#5b6cff";

    public bool IsActive { get; set; } = true;
}

public class UserManagementViewModel
{
    public List<UserSummaryViewModel> Users { get; set; } = [];
    public AdminCreateUserViewModel NewUser { get; set; } = new();
}

public class UserSummaryViewModel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? ExtraInfo { get; set; }
}

public class AdminCreateUserViewModel
{
    [Required, StringLength(120)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = RoleNames.Student;

    [Required, StringLength(40)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "P@ssword123!";

    [StringLength(150)]
    public string DepartmentOrProgramme { get; set; } = string.Empty;

    [StringLength(150)]
    public string GroupOrSpecialization { get; set; } = string.Empty;
}

public class ProposalOversightViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public int? ResearchAreaId { get; set; }
    public ProposalStatus? Status { get; set; }
    public List<ResearchAreaOptionViewModel> ResearchAreas { get; set; } = [];
    public List<ProposalOversightItemViewModel> Proposals { get; set; } = [];
}

public class ProposalOversightItemViewModel
{
    public int ProposalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ResearchArea { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; }
    public int InterestCount { get; set; }
    public string? MatchedSupervisor { get; set; }
}

public class MatchOversightViewModel
{
    public List<MatchOversightItemViewModel> Matches { get; set; } = [];
}

public class MatchOversightItemViewModel
{
    public int ProposalId { get; set; }
    public string ProposalTitle { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public DateTime ConfirmedAt { get; set; }
    public bool CreatedByAdminOverride { get; set; }
}

public class AdminReassignmentViewModel
{
    public int ProposalId { get; set; }
    public string ProposalTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Assign Supervisor")]
    public string SupervisorId { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Reason { get; set; } = string.Empty;

    public List<SelectListItem> Supervisors { get; set; } = [];
}

public class AuditLogViewModel
{
    public DateTime CreatedAt { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string UserName { get; set; } = "System";
    public string Details { get; set; } = string.Empty;
}
