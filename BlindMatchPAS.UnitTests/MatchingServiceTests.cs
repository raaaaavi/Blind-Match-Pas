using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Models.Entities;
using BlindMatchPAS.Web.Models.Enums;
using BlindMatchPAS.Web.Repositories;
using BlindMatchPAS.Web.Services;
using BlindMatchPAS.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BlindMatchPAS.UnitTests;

public class MatchingServiceTests
{
    [Fact]
    public async Task ConfirmMatchAsync_CreatesMatchAndMarksProposalAsMatched()
    {
        var context = CreateContext();
        var researchArea = new ResearchArea { Id = 1, Name = "AI", IsActive = true };
        var student = new ApplicationUser { Id = "student-1", DisplayName = "Student", Email = "student@test.local" };
        var supervisor = new ApplicationUser
        {
            Id = "supervisor-1",
            DisplayName = "Supervisor",
            Email = "supervisor@test.local",
            SupervisorProfile = new SupervisorProfile { Department = "Computing", Specialization = "AI" }
        };

        var proposal = new Proposal
        {
            Id = 100,
            Title = "Blind Matching Proposal",
            Abstract = new string('A', 120),
            TechnicalStack = "ASP.NET Core",
            ResearchAreaId = 1,
            Methodology = new string('M', 100),
            TeamSize = "3 Members",
            StudentOwnerId = student.Id,
            Status = ProposalStatus.PendingReview,
            ResearchArea = researchArea,
            StudentOwner = student
        };

        context.ResearchAreas.Add(researchArea);
        context.Users.AddRange(student, supervisor);
        context.Proposals.Add(proposal);
        await context.SaveChangesAsync();

        var service = new MatchingService(
            context,
            new ProposalRepository(context),
            new ResearchAreaRepository(context),
            Mock.Of<IAuditService>());

        var result = await service.ConfirmMatchAsync(supervisor.Id, proposal.Id);
        var refreshed = await context.Proposals.Include(x => x.Match).FirstAsync(x => x.Id == proposal.Id);

        Assert.True(result.Success);
        Assert.True(refreshed.IsMatched);
        Assert.Equal(ProposalStatus.Matched, refreshed.Status);
        Assert.NotNull(refreshed.Match);
        Assert.Equal(supervisor.Id, refreshed.Match!.SupervisorId);
    }

    [Fact]
    public async Task ConfirmMatchAsync_PreventsDoubleMatchConflicts()
    {
        var context = CreateContext();
        var researchArea = new ResearchArea { Id = 2, Name = "Cyber", IsActive = true };
        var student = new ApplicationUser { Id = "student-2", DisplayName = "Student 2" };
        var proposal = new Proposal
        {
            Id = 200,
            Title = "Already Matched Proposal",
            Abstract = new string('A', 120),
            TechnicalStack = "SQL Server",
            ResearchAreaId = 2,
            Methodology = new string('M', 100),
            TeamSize = "2 Members",
            StudentOwnerId = student.Id,
            IsMatched = true,
            Status = ProposalStatus.Matched,
            ResearchArea = researchArea,
            StudentOwner = student,
            Match = new BlindMatchPAS.Web.Models.Entities.Match { SupervisorId = "supervisor-existing" }
        };

        context.ResearchAreas.Add(researchArea);
        context.Users.Add(student);
        context.Proposals.Add(proposal);
        await context.SaveChangesAsync();

        var service = new MatchingService(
            context,
            new ProposalRepository(context),
            new ResearchAreaRepository(context),
            Mock.Of<IAuditService>());

        var result = await service.ConfirmMatchAsync("supervisor-new", proposal.Id);

        Assert.False(result.Success);
        Assert.Equal("This proposal already has a confirmed match.", result.Message);
    }

    [Fact]
    public async Task ExpressInterestAsync_PreventsDuplicateInterest()
    {
        var context = CreateContext();
        var researchArea = new ResearchArea { Id = 3, Name = "IoT", IsActive = true };
        var student = new ApplicationUser { Id = "student-3", DisplayName = "Student 3" };
        var proposal = new Proposal
        {
            Id = 300,
            Title = "IoT Proposal",
            Abstract = new string('A', 120),
            TechnicalStack = "MQTT",
            ResearchAreaId = 3,
            Methodology = new string('M', 100),
            TeamSize = "2 Members",
            StudentOwnerId = student.Id,
            Status = ProposalStatus.PendingReview,
            ResearchArea = researchArea,
            StudentOwner = student
        };
        proposal.SupervisorInterests.Add(new SupervisorInterest { SupervisorId = "supervisor-1" });

        context.ResearchAreas.Add(researchArea);
        context.Users.Add(student);
        context.Proposals.Add(proposal);
        await context.SaveChangesAsync();

        var service = new MatchingService(
            context,
            new ProposalRepository(context),
            new ResearchAreaRepository(context),
            Mock.Of<IAuditService>());

        var result = await service.ExpressInterestAsync("supervisor-1", proposal.Id);

        Assert.False(result.Success);
        Assert.Equal("You have already expressed interest in this proposal.", result.Message);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
