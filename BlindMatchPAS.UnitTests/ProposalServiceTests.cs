using BlindMatchPAS.Web.Models.Entities;
using BlindMatchPAS.Web.Models.Enums;
using BlindMatchPAS.Web.Repositories.Interfaces;
using BlindMatchPAS.Web.Services;
using BlindMatchPAS.Web.Services.Interfaces;
using BlindMatchPAS.Web.ViewModels.Student;
using Moq;

namespace BlindMatchPAS.UnitTests;

public class ProposalServiceTests
{
    [Fact]
    public async Task CreateProposalAsync_ReturnsFailure_WhenResearchAreaIsInactive()
    {
        var proposalRepository = new Mock<IProposalRepository>();
        var researchAreaRepository = new Mock<IResearchAreaRepository>();
        var auditService = new Mock<IAuditService>();

        researchAreaRepository
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(new ResearchArea { Id = 10, Name = "AI", IsActive = false });

        var service = new ProposalService(proposalRepository.Object, researchAreaRepository.Object, auditService.Object);

        var result = await service.CreateProposalAsync("student-1", new ProposalFormViewModel
        {
            Title = "Predictive Analytics Platform",
            Abstract = new string('A', 120),
            TechnicalStack = "ASP.NET Core, SQL Server",
            ResearchAreaId = 10,
            Methodology = new string('M', 80),
            TeamSize = "3 Members",
            Keywords = "analytics, ai"
        });

        Assert.False(result.Success);
        Assert.Equal("Please select an active research area.", result.Message);
        proposalRepository.Verify(x => x.AddAsync(It.IsAny<Proposal>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProposalAsync_ReturnsFailure_WhenProposalAlreadyMatched()
    {
        var proposalRepository = new Mock<IProposalRepository>();
        var researchAreaRepository = new Mock<IResearchAreaRepository>();
        var auditService = new Mock<IAuditService>();

        proposalRepository
            .Setup(x => x.GetByIdForStudentAsync(4, "student-1"))
            .ReturnsAsync(new Proposal
            {
                Id = 4,
                StudentOwnerId = "student-1",
                Title = "Matched Proposal",
                IsMatched = true,
                Status = ProposalStatus.Matched
            });

        var service = new ProposalService(proposalRepository.Object, researchAreaRepository.Object, auditService.Object);

        var result = await service.UpdateProposalAsync("student-1", 4, new ProposalFormViewModel
        {
            Title = "Updated",
            Abstract = new string('A', 120),
            TechnicalStack = "ASP.NET Core",
            ResearchAreaId = 1,
            Methodology = new string('M', 80),
            TeamSize = "2 Members"
        });

        Assert.False(result.Success);
        Assert.Equal("Matched or withdrawn proposals can no longer be edited.", result.Message);
        proposalRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task WithdrawProposalAsync_ReturnsFailure_WhenProposalAlreadyMatched()
    {
        var proposalRepository = new Mock<IProposalRepository>();
        var researchAreaRepository = new Mock<IResearchAreaRepository>();
        var auditService = new Mock<IAuditService>();

        proposalRepository
            .Setup(x => x.GetByIdForStudentAsync(8, "student-1"))
            .ReturnsAsync(new Proposal
            {
                Id = 8,
                StudentOwnerId = "student-1",
                Title = "Confirmed Proposal",
                IsMatched = true,
                Status = ProposalStatus.Matched
            });

        var service = new ProposalService(proposalRepository.Object, researchAreaRepository.Object, auditService.Object);

        var result = await service.WithdrawProposalAsync("student-1", 8);

        Assert.False(result.Success);
        Assert.Equal("Matched proposals cannot be withdrawn.", result.Message);
        auditService.Verify(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
