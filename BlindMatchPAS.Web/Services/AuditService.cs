using BlindMatchPAS.Web.Repositories.Interfaces;
using BlindMatchPAS.Web.Services.Interfaces;

namespace BlindMatchPAS.Web.Services;

public class AuditService(IAuditLogRepository auditLogRepository) : IAuditService
{
    //bug fix audit log 
    public async Task RecordAsync(string action, string entityName, string entityId, string? userId, string details)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserId = userId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });

        await auditLogRepository.SaveChangesAsync();
    }
}
