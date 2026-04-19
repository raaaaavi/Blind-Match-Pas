using BlindMatchPAS.Web.Models.Entities;
using BlindMatchPAS.Web.Models.Enums;
using BlindMatchPAS.Web.Repositories.Interfaces;
using BlindMatchPAS.Web.Services;
using BlindMatchPAS.Web.Services.Interfaces;
using BlindMatchPAS.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BlindMatchPAS.UnitTests;

public class AdminServiceTests
{
    [Fact]
    public async Task ReassignAsync_CreatesAdminOverrideMatch_WhenProposalWasUnmatched()
    {
        var proposal = new Proposal
        {
            Id = 10,
            Title = "Manual Allocation Candidate",
            Abstract = new string('A', 120),
            TechnicalStack = "ASP.NET Core",
            ResearchAreaId = 1,
            Methodology = new string('M', 100),
            TeamSize = "3 Members",
            StudentOwnerId = "student-1",
            Status = ProposalStatus.PendingReview
        };

        var researchRepo = Mock.Of<IResearchAreaRepository>();
        var auditLogRepo = Mock.Of<IAuditLogRepository>();
        var auditService = new Mock<IAuditService>();
        var proposalRepository = new Mock<IProposalRepository>();
        proposalRepository.Setup(x => x.GetByIdAsync(proposal.Id)).ReturnsAsync(proposal);
        proposalRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var userManager = MockUserManager();
        userManager
            .Setup(x => x.FindByIdAsync("supervisor-1"))
            .ReturnsAsync(new ApplicationUser { Id = "supervisor-1", DisplayName = "Supervisor One" });

        var adminService = new AdminService(
            null!,
            userManager.Object,
            researchRepo,
            proposalRepository.Object,
            auditLogRepo,
            auditService.Object);

        var result = await adminService.ReassignAsync(new AdminReassignmentViewModel
        {
            ProposalId = proposal.Id,
            ProposalTitle = proposal.Title,
            SupervisorId = "supervisor-1",
            Reason = "Coordinator assignment"
        }, "admin-1");

        Assert.True(result.Success);
        Assert.True(proposal.IsMatched);
        Assert.Equal(ProposalStatus.Matched, proposal.Status);
        Assert.NotNull(proposal.Match);
        Assert.True(proposal.Match!.CreatedByAdminOverride);
        Assert.Contains(proposal.StatusHistory, x => x.Note.Contains("Coordinator assignment"));
        proposalRepository.Verify(x => x.Update(proposal), Times.Once);
        proposalRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }
}
