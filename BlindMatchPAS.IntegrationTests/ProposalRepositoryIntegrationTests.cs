using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Models.Entities;
using BlindMatchPAS.Web.Models.Enums;
using BlindMatchPAS.Web.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.IntegrationTests;

public class ProposalRepositoryIntegrationTests
{
    [Fact]
    public async Task GetAnonymousAvailableForSupervisorAsync_ExcludesMatchedAndWithdrawnProposals()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var area = new ResearchArea { Name = "Software Engineering", IsActive = true, AccentColor = "#123456" };
        var student = new ApplicationUser { Id = "student-1", UserName = "student@test", DisplayName = "Student One" };
        var supervisor = new ApplicationUser { Id = "supervisor-1", UserName = "supervisor@test", DisplayName = "Supervisor One" };
        context.Users.AddRange(student, supervisor);
        context.ResearchAreas.Add(area);
        await context.SaveChangesAsync();

        context.Proposals.AddRange(
            new Proposal
            {
                Title = "Available Proposal",
                Abstract = new string('A', 120),
                TechnicalStack = "ASP.NET Core",
                ResearchAreaId = area.Id,
                Methodology = new string('M', 100),
                TeamSize = "3 Members",
                StudentOwnerId = student.Id,
                Status = ProposalStatus.PendingReview
            },
            new Proposal
            {
                Title = "Matched Proposal",
                Abstract = new string('A', 120),
                TechnicalStack = "SQL Server",
                ResearchAreaId = area.Id,
                Methodology = new string('M', 100),
                TeamSize = "2 Members",
                StudentOwnerId = student.Id,
                Status = ProposalStatus.Matched,
                IsMatched = true,
                Match = new BlindMatchPAS.Web.Models.Entities.Match { SupervisorId = supervisor.Id }
            },
            new Proposal
            {
                Title = "Withdrawn Proposal",
                Abstract = new string('A', 120),
                TechnicalStack = "Docker",
                ResearchAreaId = area.Id,
                Methodology = new string('M', 100),
                TeamSize = "2 Members",
                StudentOwnerId = student.Id,
                Status = ProposalStatus.Withdrawn,
                IsWithdrawn = true
            });

        await context.SaveChangesAsync();

        var repository = new ProposalRepository(context);
        var proposals = await repository.GetAnonymousAvailableForSupervisorAsync(null, null);

        Assert.Single(proposals);
        Assert.Equal("Available Proposal", proposals[0].Title);
    }

    [Fact]
    public async Task ProposalRepository_PersistsAndRetrievesKeywords()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var area = new ResearchArea { Name = "AI", IsActive = true, AccentColor = "#654321" };
        var student = new ApplicationUser { Id = "student-2", UserName = "student2@test", DisplayName = "Student Two" };
        context.Users.Add(student);
        context.ResearchAreas.Add(area);
        await context.SaveChangesAsync();

        var proposal = new Proposal
        {
            Title = "Keyword Proposal",
            Abstract = new string('A', 120),
            TechnicalStack = "Python, ASP.NET Core",
            ResearchAreaId = area.Id,
            Methodology = new string('M', 100),
            TeamSize = "2 Members",
            StudentOwnerId = student.Id,
            Status = ProposalStatus.Submitted
        };
        proposal.Keywords.Add(new ProposalKeyword { Keyword = "NLP" });
        proposal.Keywords.Add(new ProposalKeyword { Keyword = "Recommendation" });

        var repository = new ProposalRepository(context);
        await repository.AddAsync(proposal);
        await repository.SaveChangesAsync();

        var saved = await repository.GetByIdAsync(proposal.Id);

        Assert.NotNull(saved);
        Assert.Equal(2, saved!.Keywords.Count);
    }
}
