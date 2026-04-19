namespace BlindMatchPAS.Web.Repositories.Interfaces;

public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(int id);
    Task<Proposal?> GetByIdForStudentAsync(int id, string studentUserId);
    Task<List<Proposal>> GetStudentProposalsAsync(string studentUserId);
    Task<List<Proposal>> GetAnonymousAvailableForSupervisorAsync(int? researchAreaId, string? searchTerm);
    Task<List<Proposal>> GetInterestedProposalsAsync(string supervisorUserId);
    Task<List<Proposal>> GetMatchedProposalsAsync(string supervisorUserId);
    Task<List<Proposal>> GetAllAsync(string? searchTerm, int? researchAreaId, ProposalStatus? status);
    Task AddAsync(Proposal proposal);
    void Update(Proposal proposal);
    Task SaveChangesAsync();
}

public interface IResearchAreaRepository
{
    Task<List<ResearchArea>> GetActiveAsync();
    Task<List<ResearchArea>> GetAllAsync();
    Task<ResearchArea?> GetByIdAsync(int id);
    Task AddAsync(ResearchArea researchArea);
    Task SaveChangesAsync();
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<List<AuditLog>> GetRecentAsync(int count = 200);
    Task SaveChangesAsync();
}
