using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Repositories.Interfaces;
using BlindMatchPAS.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Services;

public class AdminService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IResearchAreaRepository researchAreaRepository,
    IProposalRepository proposalRepository,
    IAuditLogRepository auditLogRepository,
    IAuditService auditService) : IAdminService
{
    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var students = await userManager.GetUsersInRoleAsync(RoleNames.Student);
        var supervisors = await userManager.GetUsersInRoleAsync(RoleNames.Supervisor);

        return new AdminDashboardViewModel
        {
            TotalStudents = students.Count,
            TotalSupervisors = supervisors.Count,
            TotalProposals = await context.Proposals.CountAsync(),
            TotalMatchedProjects = await context.Proposals.CountAsync(x => x.IsMatched),
            PendingReviews = await context.Proposals.CountAsync(x => x.Status == ProposalStatus.PendingReview || x.Status == ProposalStatus.UnderReview || x.Status == ProposalStatus.Submitted),
            ProposalsByArea = await context.Proposals.Include(x => x.ResearchArea)
                .GroupBy(x => x.ResearchArea!.Name)
                .Select(x => new AreaAnalyticsViewModel { AreaName = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            RecentAuditLogs = (await GetAuditLogsAsync()).Take(8).ToList()
        };
    }

    public async Task<ResearchAreaManagementViewModel> GetResearchAreasAsync()
    {
        var areas = await researchAreaRepository.GetAllAsync();
        return new ResearchAreaManagementViewModel
        {
            ResearchAreas = areas.Select(x => new ResearchAreaEditViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                AccentColor = x.AccentColor,
                IsActive = x.IsActive
            }).ToList(),
            NewResearchArea = new ResearchAreaEditViewModel()
        };
    }

    public async Task<ServiceResult> SaveResearchAreaAsync(ResearchAreaEditViewModel model)
    {
        if (model.Id == 0)
        {
            await researchAreaRepository.AddAsync(new ResearchArea
            {
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                AccentColor = model.AccentColor,
                IsActive = model.IsActive
            });
        }
        else
        {
            var area = await researchAreaRepository.GetByIdAsync(model.Id);
            if (area is null)
            {
                return ServiceResult.Fail("Research area not found.");
            }

            area.Name = model.Name.Trim();
            area.Description = model.Description.Trim();
            area.AccentColor = model.AccentColor;
            area.IsActive = model.IsActive;
        }

        await researchAreaRepository.SaveChangesAsync();
        return ServiceResult.Ok("Research area saved successfully.");
    }

    public async Task<ServiceResult> ToggleResearchAreaAsync(int id)
    {
        var area = await researchAreaRepository.GetByIdAsync(id);
        if (area is null)
        {
            return ServiceResult.Fail("Research area not found.");
        }

        area.IsActive = !area.IsActive;
        await researchAreaRepository.SaveChangesAsync();
        return ServiceResult.Ok("Research area status updated.");
    }

    public async Task<UserManagementViewModel> GetUsersAsync()
    {
        var users = await context.Users
            .Include(x => x.StudentProfile)
            .Include(x => x.SupervisorProfile)
            .OrderBy(x => x.DisplayName)
            .ToListAsync();

        var viewModel = new UserManagementViewModel();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            viewModel.Users.Add(new UserSummaryViewModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Unassigned",
                IsActive = user.IsActive,
                ExtraInfo = user.StudentProfile?.Programme ?? user.SupervisorProfile?.Specialization ?? user.RegistrationNumber
            });
        }

        return viewModel;
    }

    public async Task<ServiceResult> CreateUserAsync(AdminCreateUserViewModel model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName,
            RegistrationNumber = model.RegistrationNumber,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return ServiceResult.Fail(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        await userManager.AddToRoleAsync(user, model.Role);

        if (model.Role == RoleNames.Student)
        {
            context.StudentProfiles.Add(new StudentProfile
            {
                UserId = user.Id,
                StudentIdentifier = model.RegistrationNumber,
                Programme = model.DepartmentOrProgramme,
                GroupName = model.GroupOrSpecialization,
                TeamMemberNames = model.DisplayName
            });
        }
        else if (model.Role == RoleNames.Supervisor)
        {
            context.SupervisorProfiles.Add(new SupervisorProfile
            {
                UserId = user.Id,
                Department = string.IsNullOrWhiteSpace(model.DepartmentOrProgramme) ? "Computing" : model.DepartmentOrProgramme,
                Specialization = string.IsNullOrWhiteSpace(model.GroupOrSpecialization) ? "Project Supervision" : model.GroupOrSpecialization,
                OfficeLocation = "TBD"
            });
        }

        await context.SaveChangesAsync();
        return ServiceResult.Ok("User account created successfully.");
    }

    public async Task<ServiceResult> ToggleUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult.Fail("User not found.");
        }

        user.IsActive = !user.IsActive;
        await userManager.UpdateAsync(user);
        return ServiceResult.Ok("User activation status updated.");
    }

    public async Task<ProposalOversightViewModel> GetProposalOversightAsync(string? searchTerm, int? researchAreaId, ProposalStatus? status)
    {
        var proposals = await proposalRepository.GetAllAsync(searchTerm, researchAreaId, status);
        var areas = await researchAreaRepository.GetAllAsync();
        return new ProposalOversightViewModel
        {
            SearchTerm = searchTerm ?? string.Empty,
            ResearchAreaId = researchAreaId,
            Status = status,
            ResearchAreas = areas.Select(x => new ResearchAreaOptionViewModel { Id = x.Id, Name = x.Name, AccentColor = x.AccentColor }).ToList(),
            Proposals = proposals.Select(x => new ProposalOversightItemViewModel
            {
                ProposalId = x.Id,
                Title = x.Title,
                StudentName = x.StudentOwner?.DisplayName ?? "Unknown",
                ResearchArea = x.ResearchArea?.Name ?? string.Empty,
                Status = x.Status,
                InterestCount = x.SupervisorInterests.Count,
                MatchedSupervisor = x.Match?.Supervisor?.DisplayName
            }).ToList()
        };
    }

    public async Task<MatchOversightViewModel> GetMatchOversightAsync()
    {
        var matches = await context.Matches
            .Include(x => x.Proposal)
                .ThenInclude(x => x!.StudentOwner)
            .Include(x => x.Supervisor)
            .OrderByDescending(x => x.ConfirmedAt)
            .ToListAsync();

        return new MatchOversightViewModel
        {
            Matches = matches.Select(x => new MatchOversightItemViewModel
            {
                ProposalId = x.ProposalId,
                ProposalTitle = x.Proposal?.Title ?? string.Empty,
                StudentName = x.Proposal?.StudentOwner?.DisplayName ?? string.Empty,
                SupervisorName = x.Supervisor?.DisplayName ?? string.Empty,
                ConfirmedAt = x.ConfirmedAt,
                CreatedByAdminOverride = x.CreatedByAdminOverride
            }).ToList()
        };
    }

    public async Task<AdminReassignmentViewModel?> BuildReassignmentViewModelAsync(int proposalId)
    {
        var proposal = await proposalRepository.GetByIdAsync(proposalId);
        if (proposal is null)
        {
            return null;
        }

        var supervisors = await userManager.GetUsersInRoleAsync(RoleNames.Supervisor);
        return new AdminReassignmentViewModel
        {
            ProposalId = proposal.Id,
            ProposalTitle = proposal.Title,
            SupervisorId = proposal.Match?.SupervisorId ?? string.Empty,
            Supervisors = supervisors.OrderBy(x => x.DisplayName).Select(x => new SelectListItem(x.DisplayName, x.Id)).ToList()
        };
    }

    public async Task<ServiceResult> ReassignAsync(AdminReassignmentViewModel model, string actingUserId)
    {
        var proposal = await proposalRepository.GetByIdAsync(model.ProposalId);
        if (proposal is null)
        {
            return ServiceResult.Fail("Proposal not found.");
        }

        var supervisor = await userManager.FindByIdAsync(model.SupervisorId);
        if (supervisor is null)
        {
            return ServiceResult.Fail("Supervisor not found.");
        }

        if (proposal.Match is null)
        {
            proposal.Match = new Match
            {
                ProposalId = proposal.Id,
                SupervisorId = supervisor.Id,
                ConfirmedAt = DateTime.UtcNow,
                RevealedAt = DateTime.UtcNow,
                Status = MatchStatus.Confirmed,
                CreatedByAdminOverride = true
            };
        }
        else
        {
            proposal.Match.SupervisorId = supervisor.Id;
            proposal.Match.ConfirmedAt = DateTime.UtcNow;
            proposal.Match.RevealedAt = DateTime.UtcNow;
            proposal.Match.Status = MatchStatus.Reassigned;
            proposal.Match.CreatedByAdminOverride = true;
        }

        proposal.IsMatched = true;
        proposal.Status = ProposalStatus.Matched;
        proposal.UpdatedAt = DateTime.UtcNow;
        proposal.StatusHistory.Add(new ProposalStatusHistory
        {
            Status = ProposalStatus.Matched,
            Note = $"Admin override assignment applied. Reason: {model.Reason}",
            ChangedByUserId = actingUserId,
            ChangedAt = DateTime.UtcNow
        });

        if (!proposal.SupervisorInterests.Any(x => x.SupervisorId == supervisor.Id))
        {
            proposal.SupervisorInterests.Add(new SupervisorInterest
            {
                ProposalId = proposal.Id,
                SupervisorId = supervisor.Id,
                CreatedAt = DateTime.UtcNow,
                IsConfirmed = true
            });
        }

        proposalRepository.Update(proposal);
        await proposalRepository.SaveChangesAsync();
        await auditService.RecordAsync("Admin reassignment", nameof(Proposal), proposal.Id.ToString(), actingUserId, model.Reason);
        return ServiceResult.Ok("Supervisor assignment updated successfully.");
    }

    public async Task<List<AuditLogViewModel>> GetAuditLogsAsync()
    {
        var logs = await auditLogRepository.GetRecentAsync();
        return logs.Select(x => new AuditLogViewModel
        {
            CreatedAt = x.CreatedAt,
            Action = x.Action,
            EntityName = x.EntityName,
            EntityId = x.EntityId,
            UserName = x.User?.DisplayName ?? "System",
            Details = x.Details
        }).ToList();
    }
}
