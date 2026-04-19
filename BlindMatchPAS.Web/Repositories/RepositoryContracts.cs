namespace BlindMatchPAS.Web.Repositories.Interfaces;

/// ============================================================================
/// REPOSITORY CONTRACTS - Data Access Layer Interfaces
/// ============================================================================
/// Repositories abstract database queries and provide a clean API for services.
/// Each repository handles CRUD operations for specific entities.
/// Implementation: Repositories.cs
/// Database: ApplicationDbContext with Entity Framework Core
/// ============================================================================

/// <summary>
/// IProposalRepository - Data access for Proposal entity
/// 
/// Responsibilities:
/// - Retrieve proposals with various filters
/// - Support student, supervisor, and admin queries
/// - Enforce blind-review constraints (anonymous proposal queries)
/// - Provide persistence methods (Add, Update, Save)
/// 
/// Key Methods:
/// - GetByIdAsync(id) - Get single proposal
/// - GetStudentProposalsAsync(studentId) - Get student's own proposals
/// - GetAnonymousAvailableForSupervisorAsync(...) - Blind review query
/// - GetInterestedProposalsAsync(supervisorId) - Interest tracking
/// - GetMatchedProposalsAsync(supervisorId) - Confirmed matches
/// - GetAllAsync(...) - Admin overview with filters
/// </summary>
public interface IProposalRepository
{
    /// <summary>Get proposal by ID (any user can access)</summary>
    Task<Proposal?> GetByIdAsync(int id);
    
    /// <summary>Get proposal for specific student (ownership check)</summary>
    Task<Proposal?> GetByIdForStudentAsync(int id, string studentUserId);
    
    /// <summary>Get all proposals owned by a student</summary>
    Task<List<Proposal>> GetStudentProposalsAsync(string studentUserId);
    
    /// <summary>Get proposals available for supervisor review (anonymous, excludes matched/withdrawn)</summary>
    Task<List<Proposal>> GetAnonymousAvailableForSupervisorAsync(int? researchAreaId, string? searchTerm);
    
    /// <summary>Get proposals supervisor has expressed interest in</summary>
    Task<List<Proposal>> GetInterestedProposalsAsync(string supervisorUserId);
    
    /// <summary>Get proposals with confirmed matches for supervisor</summary>
    Task<List<Proposal>> GetMatchedProposalsAsync(string supervisorUserId);
    
    /// <summary>Get all proposals (admin overview, optionally filtered)</summary>
    Task<List<Proposal>> GetAllAsync(string? searchTerm, int? researchAreaId, ProposalStatus? status);
    
    /// <summary>Insert new proposal into database</summary>
    Task AddAsync(Proposal proposal);
    
    /// <summary>Mark proposal as modified (call before SaveChangesAsync)</summary>
    void Update(Proposal proposal);
    
    /// <summary>Persist all changes to database</summary>
    Task SaveChangesAsync();
}

/// <summary>
/// IResearchAreaRepository - Data access for ResearchArea entity
/// 
/// Responsibilities:
/// - Retrieve research areas for categorization
/// - Distinguish active vs. inactive areas
/// - Support area management CRUD operations
/// 
/// Key Methods:
/// - GetActiveAsync() - Active areas for student/supervisor selection
/// - GetAllAsync() - All areas including inactive (admin)
/// - GetByIdAsync(id) - Single area lookup
/// - AddAsync() - Create new research area
/// </summary>
public interface IResearchAreaRepository
{
    /// <summary>Get all active research areas (for student/supervisor dropdown)</summary>
    Task<List<ResearchArea>> GetActiveAsync();
    
    /// <summary>Get all research areas including inactive ones (admin view)</summary>
    Task<List<ResearchArea>> GetAllAsync();
    
    /// <summary>Get single research area by ID</summary>
    Task<ResearchArea?> GetByIdAsync(int id);
    
    /// <summary>Insert new research area into database</summary>
    Task AddAsync(ResearchArea researchArea);
    
    /// <summary>Persist changes to database</summary>
    Task SaveChangesAsync();
}

/// <summary>
/// IAuditLogRepository - Data access for AuditLog entity
/// 
/// Responsibilities:
/// - Record system audit events
/// - Retrieve audit history for compliance
/// - Support audit trail queries
/// 
/// Key Methods:
/// - AddAsync() - Record action to audit log
/// - GetRecentAsync(count) - Get recent audit logs
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>Insert audit log entry into database</summary>
    Task AddAsync(AuditLog log);
    
    /// <summary>Get most recent audit logs (ordered by date descending)</summary>
    Task<List<AuditLog>> GetRecentAsync(int count = 200);
    
    /// <summary>Persist changes to database</summary>
    Task SaveChangesAsync();
}
