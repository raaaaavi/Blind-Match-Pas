namespace BlindMatchPAS.Web.Services.Interfaces;

public interface IProposalService
{
    Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentUserId);
    Task<List<ProposalListItemViewModel>> GetStudentProposalsAsync(string studentUserId);
    Task<ProposalFormViewModel> BuildProposalFormAsync(string studentUserId, int? proposalId = null);
    Task<ServiceResult<int>> CreateProposalAsync(string studentUserId, ProposalFormViewModel model);
    Task<ServiceResult> UpdateProposalAsync(string studentUserId, int proposalId, ProposalFormViewModel model);
    Task<ServiceResult> WithdrawProposalAsync(string studentUserId, int proposalId);
    Task<ProposalDetailsViewModel?> GetStudentProposalDetailsAsync(string studentUserId, int proposalId);
}

public interface IMatchingService
{
    Task<SupervisorDashboardViewModel> GetSupervisorDashboardAsync(string supervisorUserId);
    Task<AnonymousProposalBrowserViewModel> GetAnonymousBrowserAsync(int? researchAreaId, string? searchTerm);
    Task<SupervisorProposalDetailsViewModel?> GetSupervisorProposalDetailsAsync(string supervisorUserId, int proposalId);
    Task<List<SupervisorProposalCardViewModel>> GetInterestedAsync(string supervisorUserId);
    Task<List<SupervisorProposalCardViewModel>> GetConfirmedMatchesAsync(string supervisorUserId);
    Task<ExpertiseManagementViewModel> BuildExpertiseViewModelAsync(string supervisorUserId);
    Task<ServiceResult> UpdateExpertiseAsync(string supervisorUserId, ExpertiseManagementViewModel model);
    Task<ServiceResult> ExpressInterestAsync(string supervisorUserId, int proposalId);
    Task<ServiceResult> ConfirmMatchAsync(string supervisorUserId, int proposalId);
}

public interface IAdminService
{
    Task<AdminDashboardViewModel> GetDashboardAsync();
    Task<ResearchAreaManagementViewModel> GetResearchAreasAsync();
    Task<ServiceResult> SaveResearchAreaAsync(ResearchAreaEditViewModel model);
    Task<ServiceResult> ToggleResearchAreaAsync(int id);
    Task<UserManagementViewModel> GetUsersAsync();
    Task<ServiceResult> CreateUserAsync(AdminCreateUserViewModel model);
    Task<ServiceResult> ToggleUserAsync(string userId);
    Task<ProposalOversightViewModel> GetProposalOversightAsync(string? searchTerm, int? researchAreaId, ProposalStatus? status);
    Task<MatchOversightViewModel> GetMatchOversightAsync();
    Task<AdminReassignmentViewModel?> BuildReassignmentViewModelAsync(int proposalId);
    Task<ServiceResult> ReassignAsync(AdminReassignmentViewModel model, string actingUserId);
    Task<List<AuditLogViewModel>> GetAuditLogsAsync();
}

public interface IAuditService
{
    Task RecordAsync(string action, string entityName, string entityId, string? userId, string details);
}
