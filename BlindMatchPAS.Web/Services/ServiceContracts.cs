namespace BlindMatchPAS.Web.Services.Interfaces;

/// ============================================================================
/// SERVICE CONTRACTS - Define all business logic interfaces
/// ============================================================================
/// These interfaces define the public API for the business logic layer.
/// All services are injected via dependency injection into controllers.
/// Implementation files: ProposalService.cs, MatchingService.cs, etc.
/// ============================================================================

/// <summary>
/// IProposalService - Handles student proposal lifecycle management
/// 
/// Responsibilities:
/// - Proposal creation, editing, and withdrawal
/// - Proposal validation and state transition management
/// - Student dashboard and proposal listing
/// - View model construction for student-facing views
/// </summary>
public interface IProposalService
{
    /// <summary>Get student dashboard with proposal summary and statistics</summary>
    Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentUserId);
    
    /// <summary>Get all proposals owned by the student</summary>
    Task<List<ProposalListItemViewModel>> GetStudentProposalsAsync(string studentUserId);
    
    /// <summary>Build proposal form view model (create or edit mode)</summary>
    Task<ProposalFormViewModel> BuildProposalFormAsync(string studentUserId, int? proposalId = null);
    
    /// <summary>Create a new proposal - validates and persists to database</summary>
    Task<ServiceResult<int>> CreateProposalAsync(string studentUserId, ProposalFormViewModel model);
    
    /// <summary>Update existing proposal - only if not matched or withdrawn</summary>
    Task<ServiceResult> UpdateProposalAsync(string studentUserId, int proposalId, ProposalFormViewModel model);
    
    /// <summary>Withdraw proposal - only if not matched</summary>
    Task<ServiceResult> WithdrawProposalAsync(string studentUserId, int proposalId);
    
    /// <summary>Get proposal details for student view (shows student-owned info)</summary>
    Task<ProposalDetailsViewModel?> GetStudentProposalDetailsAsync(string studentUserId, int proposalId);
}

/// <summary>
/// IMatchingService - Handles blind proposal review and matching
/// 
/// Key Features:
/// - Anonymous proposal browsing for supervisors
/// - Supervisor interest expression (without revealing student identity)
/// - Match confirmation and identity reveal
/// - Supervisor expertise management
/// 
/// Responsibilities:
/// - Filter proposals for anonymous browsing
/// - Manage supervisor interests
/// - Coordinate match confirmation workflow
/// - Handle identity reveal timing
/// </summary>
public interface IMatchingService
{
    /// <summary>Get supervisor dashboard with summary of interests and matches</summary>
    Task<SupervisorDashboardViewModel> GetSupervisorDashboardAsync(string supervisorUserId);
    
    /// <summary>Get anonymous proposals for supervisor browsing (student identity hidden)</summary>
    Task<AnonymousProposalBrowserViewModel> GetAnonymousBrowserAsync(int? researchAreaId, string? searchTerm);
    
    /// <summary>Get proposal details for supervisor view (anonymous, before match)</summary>
    Task<SupervisorProposalDetailsViewModel?> GetSupervisorProposalDetailsAsync(string supervisorUserId, int proposalId);
    
    /// <summary>Get all proposals supervisor has expressed interest in</summary>
    Task<List<SupervisorProposalCardViewModel>> GetInterestedAsync(string supervisorUserId);
    
    /// <summary>Get all confirmed matches for supervisor</summary>
    Task<List<SupervisorProposalCardViewModel>> GetConfirmedMatchesAsync(string supervisorUserId);
    
    /// <summary>Build view model for supervisor expertise management</summary>
    Task<ExpertiseManagementViewModel> BuildExpertiseViewModelAsync(string supervisorUserId);
    
    /// <summary>Update supervisor's expertise areas</summary>
    Task<ServiceResult> UpdateExpertiseAsync(string supervisorUserId, ExpertiseManagementViewModel model);
    
    /// <summary>Express interest in a proposal (anonymous interest, can be withdrawn)</summary>
    Task<ServiceResult> ExpressInterestAsync(string supervisorUserId, int proposalId);
    
    /// <summary>Confirm match with proposal (identity reveal occurs here)</summary>
    Task<ServiceResult> ConfirmMatchAsync(string supervisorUserId, int proposalId);
}

/// <summary>
/// IAdminService - Handles administrative operations
/// 
/// Responsibilities:
/// - System dashboards and oversight
/// - Research area management (CRUD)
/// - User account management
/// - Proposal reassignment with audit trail
/// - Audit log management
/// 
/// Features:
/// - Proposal overview with filtering
/// - Match oversight and statistics
/// - Admin reassignment with reason tracking
/// - Comprehensive audit logging
/// </summary>
public interface IAdminService
{
    /// <summary>Get admin dashboard with system overview</summary>
    Task<AdminDashboardViewModel> GetDashboardAsync();
    
    /// <summary>Get all research areas with active/inactive status</summary>
    Task<ResearchAreaManagementViewModel> GetResearchAreasAsync();
    
    /// <summary>Create or update a research area</summary>
    Task<ServiceResult> SaveResearchAreaAsync(ResearchAreaEditViewModel model);
    
    /// <summary>Activate or deactivate a research area</summary>
    Task<ServiceResult> ToggleResearchAreaAsync(int id);
    
    /// <summary>Get all users with roles and status</summary>
    Task<UserManagementViewModel> GetUsersAsync();
    
    /// <summary>Create a new user account with assigned role</summary>
    Task<ServiceResult> CreateUserAsync(AdminCreateUserViewModel model);
    
    /// <summary>Enable or disable user account</summary>
    Task<ServiceResult> ToggleUserAsync(string userId);
    
    /// <summary>Get all proposals with optional filtering</summary>
    Task<ProposalOversightViewModel> GetProposalOversightAsync(string? searchTerm, int? researchAreaId, ProposalStatus? status);
    
    /// <summary>Get all matches with assignment status</summary>
    Task<MatchOversightViewModel> GetMatchOversightAsync();
    
    /// <summary>Build view model for admin reassignment form</summary>
    Task<AdminReassignmentViewModel?> BuildReassignmentViewModelAsync(int proposalId);
    
    /// <summary>Reassign proposal to different supervisor (override)</summary>
    Task<ServiceResult> ReassignAsync(AdminReassignmentViewModel model, string actingUserId);
    
    /// <summary>Get system audit logs (all user actions)</summary>
    Task<List<AuditLogViewModel>> GetAuditLogsAsync();
}

/// <summary>
/// IAuditService - Handles system audit logging
/// 
/// Responsibilities:
/// - Record all critical business actions
/// - Maintain audit trail for compliance
/// - Track user actions with timestamps
/// - Support administrative oversight
/// </summary>
public interface IAuditService
{
    /// <summary>Record an action to the audit log</summary>
    Task RecordAsync(string action, string entityName, string entityId, string? userId, string details);
}
