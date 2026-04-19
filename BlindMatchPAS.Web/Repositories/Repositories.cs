using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Repositories;

public class ProposalRepository(ApplicationDbContext context) : IProposalRepository
{
    public Task<Proposal?> GetByIdAsync(int id) => Queryable().FirstOrDefaultAsync(x => x.Id == id);

    public Task<Proposal?> GetByIdForStudentAsync(int id, string studentUserId) =>
        Queryable().FirstOrDefaultAsync(x => x.Id == id && x.StudentOwnerId == studentUserId);

    public Task<List<Proposal>> GetStudentProposalsAsync(string studentUserId) =>
        Queryable().Where(x => x.StudentOwnerId == studentUserId).OrderByDescending(x => x.UpdatedAt).ToListAsync();

    public Task<List<Proposal>> GetAnonymousAvailableForSupervisorAsync(int? researchAreaId, string? searchTerm)
    {
        var query = Queryable()
            .Where(x => !x.IsMatched && !x.IsWithdrawn && x.Status != ProposalStatus.Withdrawn);

        if (researchAreaId.HasValue)
        {
            query = query.Where(x => x.ResearchAreaId == researchAreaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.Title.Contains(searchTerm) || x.Abstract.Contains(searchTerm) || x.TechnicalStack.Contains(searchTerm));
        }

        return query.OrderByDescending(x => x.SubmittedAt ?? x.UpdatedAt).ToListAsync();
    }

    public Task<List<Proposal>> GetInterestedProposalsAsync(string supervisorUserId) =>
        Queryable()
            .Where(x => x.SupervisorInterests.Any(i => i.SupervisorId == supervisorUserId))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

    public Task<List<Proposal>> GetMatchedProposalsAsync(string supervisorUserId) =>
        Queryable()
            .Where(x => x.Match != null && x.Match.SupervisorId == supervisorUserId)
            .OrderByDescending(x => x.Match!.ConfirmedAt)
            .ToListAsync();

    public Task<List<Proposal>> GetAllAsync(string? searchTerm, int? researchAreaId, ProposalStatus? status)
    {
        var query = Queryable().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.Title.Contains(searchTerm) || (x.StudentOwner != null && x.StudentOwner.DisplayName.Contains(searchTerm)));
        }

        if (researchAreaId.HasValue)
        {
            query = query.Where(x => x.ResearchAreaId == researchAreaId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return query.OrderByDescending(x => x.UpdatedAt).ToListAsync();
    }

    public Task AddAsync(Proposal proposal) => context.Proposals.AddAsync(proposal).AsTask();
    public void Update(Proposal proposal) => context.Proposals.Update(proposal);
    public Task SaveChangesAsync() => context.SaveChangesAsync();

    private IQueryable<Proposal> Queryable() => context.Proposals
        .Include(x => x.ResearchArea)
        .Include(x => x.Keywords)
        .Include(x => x.StudentOwner)
            .ThenInclude(x => x!.StudentProfile)
        .Include(x => x.SupervisorInterests)
            .ThenInclude(x => x.Supervisor)
                .ThenInclude(x => x!.SupervisorProfile)
        .Include(x => x.Match)
            .ThenInclude(x => x!.Supervisor)
                .ThenInclude(x => x!.SupervisorProfile)
        .Include(x => x.StatusHistory)
        .AsSplitQuery();
}

public class ResearchAreaRepository(ApplicationDbContext context) : IResearchAreaRepository
{
    public Task<List<ResearchArea>> GetActiveAsync() => context.ResearchAreas.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
    public Task<List<ResearchArea>> GetAllAsync() => context.ResearchAreas.OrderBy(x => x.Name).ToListAsync();
    public Task<ResearchArea?> GetByIdAsync(int id) => context.ResearchAreas.FirstOrDefaultAsync(x => x.Id == id);
    public Task AddAsync(ResearchArea researchArea) => context.ResearchAreas.AddAsync(researchArea).AsTask();
    public Task SaveChangesAsync() => context.SaveChangesAsync();
}

public class AuditLogRepository(ApplicationDbContext context) : IAuditLogRepository
{
    public Task AddAsync(AuditLog log) => context.AuditLogs.AddAsync(log).AsTask();

    public Task<List<AuditLog>> GetRecentAsync(int count = 200) => context.AuditLogs
        .Include(x => x.User)
        .OrderByDescending(x => x.CreatedAt)
        .Take(count)
        .ToListAsync();

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
