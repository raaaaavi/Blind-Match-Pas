using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Data.Seed;

public class DatabaseSeeder(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    public async Task SeedAsync()
    {
        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var sysAdmin = await EnsureUserAsync("admin@blindmatchpas.local", "System Admin", RoleNames.SystemAdmin, "PUSL-ADMIN-001");
        await EnsureUserAsync("moduleleader@blindmatchpas.local", "Dr. Nadeesha Silva", RoleNames.ModuleLeader, "PUSL-ML-001");
        var supervisor1 = await EnsureUserAsync("supervisor.ai@blindmatchpas.local", "Dr. Ayesha Fernando", RoleNames.Supervisor, "SUP-AI-001");
        var supervisor2 = await EnsureUserAsync("supervisor.cyber@blindmatchpas.local", "Prof. Kasun Perera", RoleNames.Supervisor, "SUP-CY-001");
        var supervisor3 = await EnsureUserAsync("supervisor.iot@blindmatchpas.local", "Dr. Dinithi Rajapaksha", RoleNames.Supervisor, "SUP-IOT-001");
        var student1 = await EnsureUserAsync("student.group1@blindmatchpas.local", "Team Nova", RoleNames.Student, "STU-2020-001");
        var student2 = await EnsureUserAsync("student.group2@blindmatchpas.local", "Team Vertex", RoleNames.Student, "STU-2020-002");
        var student3 = await EnsureUserAsync("student.group3@blindmatchpas.local", "Team Cipher", RoleNames.Student, "STU-2020-003");

        await EnsureProfilesAsync(student1, student2, student3, supervisor1, supervisor2, supervisor3);
        await EnsureResearchAreasAsync();
        await context.SaveChangesAsync();
        await EnsureSupervisorExpertiseAsync(supervisor1.Id, supervisor2.Id, supervisor3.Id);
        await EnsureProposalsAsync(student1.Id, student2.Id, student3.Id, supervisor1.Id, supervisor2.Id);
        await context.SaveChangesAsync();

        if (!await context.AuditLogs.AnyAsync())
        {
            context.AuditLogs.Add(new AuditLog
            {
                Action = "System seeded",
                EntityName = "Environment",
                EntityId = "initial",
                UserId = sysAdmin.Id,
                Details = "Default roles, demo users, and sample data created."
            });
            await context.SaveChangesAsync();
        }
    }

    private async Task<ApplicationUser> EnsureUserAsync(string email, string displayName, string role, string registrationNumber)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                RegistrationNumber = registrationNumber,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, "P@ssword123!");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unable to seed user {email}: {string.Join(',', result.Errors.Select(x => x.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    private async Task EnsureProfilesAsync(
        ApplicationUser student1,
        ApplicationUser student2,
        ApplicationUser student3,
        ApplicationUser supervisor1,
        ApplicationUser supervisor2,
        ApplicationUser supervisor3)
    {
        if (!await context.StudentProfiles.AnyAsync())
        {
            context.StudentProfiles.AddRange(
                new StudentProfile
                {
                    UserId = student1.Id,
                    StudentIdentifier = "PUSL2020-2020-001",
                    Programme = "BSc (Hons) Software Engineering",
                    GroupName = "Group Nova",
                    TeamMemberNames = "Nimal Perera, Sahan Wijesinghe, Thilini Jayasekara"
                },
                new StudentProfile
                {
                    UserId = student2.Id,
                    StudentIdentifier = "PUSL2020-2020-002",
                    Programme = "BSc (Hons) Software Engineering",
                    GroupName = "Group Vertex",
                    TeamMemberNames = "Madhavi Fernando, Ruwan Weerasekara, Hansi Perera"
                },
                new StudentProfile
                {
                    UserId = student3.Id,
                    StudentIdentifier = "PUSL2020-2020-003",
                    Programme = "BSc (Hons) Software Engineering",
                    GroupName = "Group Cipher",
                    TeamMemberNames = "Ishara Silva, Kavindu Jayasinghe"
                });
        }

        if (!await context.SupervisorProfiles.AnyAsync())
        {
            context.SupervisorProfiles.AddRange(
                new SupervisorProfile
                {
                    UserId = supervisor1.Id,
                    Department = "Computing and AI",
                    Specialization = "Applied Machine Learning, NLP",
                    OfficeLocation = "Innovation Wing 4.2"
                },
                new SupervisorProfile
                {
                    UserId = supervisor2.Id,
                    Department = "Cyber Security",
                    Specialization = "Digital Forensics, Secure Systems",
                    OfficeLocation = "Security Lab 2.1"
                },
                new SupervisorProfile
                {
                    UserId = supervisor3.Id,
                    Department = "Embedded and IoT",
                    Specialization = "IoT Systems, Edge Computing",
                    OfficeLocation = "Tech Hub 1.7"
                });
        }
    }

    private async Task EnsureResearchAreasAsync()
    {
        if (await context.ResearchAreas.AnyAsync())
        {
            return;
        }

        context.ResearchAreas.AddRange(
            new ResearchArea { Name = "Artificial Intelligence", Description = "Machine learning, intelligent systems, recommendation engines.", AccentColor = "#6c63ff", IsActive = true },
            new ResearchArea { Name = "Cyber Security", Description = "Threat detection, secure platforms, digital forensics.", AccentColor = "#0ea5e9", IsActive = true },
            new ResearchArea { Name = "Internet of Things", Description = "Sensors, embedded platforms, edge systems.", AccentColor = "#22c55e", IsActive = true },
            new ResearchArea { Name = "Data Science", Description = "Forecasting, analytics, insights and decision support.", AccentColor = "#f59e0b", IsActive = true },
            new ResearchArea { Name = "Software Engineering", Description = "Quality engineering, architecture, tooling and automation.", AccentColor = "#ef4444", IsActive = true });
    }

    private async Task EnsureSupervisorExpertiseAsync(string supervisor1Id, string supervisor2Id, string supervisor3Id)
    {
        if (await context.SupervisorExpertise.AnyAsync())
        {
            return;
        }

        var areas = await context.ResearchAreas.ToDictionaryAsync(x => x.Name, x => x.Id);
        context.SupervisorExpertise.AddRange(
            new SupervisorExpertise { SupervisorId = supervisor1Id, ResearchAreaId = areas["Artificial Intelligence"] },
            new SupervisorExpertise { SupervisorId = supervisor1Id, ResearchAreaId = areas["Data Science"] },
            new SupervisorExpertise { SupervisorId = supervisor2Id, ResearchAreaId = areas["Cyber Security"] },
            new SupervisorExpertise { SupervisorId = supervisor2Id, ResearchAreaId = areas["Software Engineering"] },
            new SupervisorExpertise { SupervisorId = supervisor3Id, ResearchAreaId = areas["Internet of Things"] },
            new SupervisorExpertise { SupervisorId = supervisor3Id, ResearchAreaId = areas["Software Engineering"] });
    }

    private async Task EnsureProposalsAsync(string student1Id, string student2Id, string student3Id, string supervisor1Id, string supervisor2Id)
    {
        if (await context.Proposals.AnyAsync())
        {
            return;
        }

        var areas = await context.ResearchAreas.ToDictionaryAsync(x => x.Name, x => x.Id);

        var pendingProposal = new Proposal
        {
            Title = "Blind-Matched Career Guidance Platform with Explainable Recommendations",
            Abstract = "A final-year project proposal to build an explainable career guidance platform that recommends project pathways and personalised career maps using transparent machine learning models and evidence-based feedback loops for undergraduate learners.",
            TechnicalStack = "ASP.NET Core MVC, SQL Server, Python recommendation service, Bootstrap 5, Azure DevOps",
            ResearchAreaId = areas["Artificial Intelligence"],
            Methodology = "Use iterative prototyping, model comparison, usability evaluation, and supervisor feedback checkpoints to refine the recommendation flow and ensure traceable outputs.",
            TeamSize = "3 Members",
            StudentOwnerId = student1Id,
            Status = ProposalStatus.PendingReview,
            SubmittedAt = DateTime.UtcNow.AddDays(-8),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };
        pendingProposal.Keywords.Add(new ProposalKeyword { Keyword = "Explainable AI" });
        pendingProposal.Keywords.Add(new ProposalKeyword { Keyword = "Education" });
        pendingProposal.StatusHistory.Add(new ProposalStatusHistory { Status = ProposalStatus.Submitted, Note = "Proposal submitted by group lead.", ChangedByUserId = student1Id, ChangedAt = DateTime.UtcNow.AddDays(-8) });
        pendingProposal.StatusHistory.Add(new ProposalStatusHistory { Status = ProposalStatus.PendingReview, Note = "Ready for blind supervisor review.", ChangedByUserId = student1Id, ChangedAt = DateTime.UtcNow.AddDays(-8).AddHours(1) });

        var matchedProposal = new Proposal
        {
            Title = "Privacy-Preserving Threat Intelligence Dashboard for Campus Networks",
            Abstract = "A secure monitoring dashboard that aggregates anonymised campus network events and applies privacy-preserving analytics to identify emerging threats without disclosing sensitive student-level identities during review and triage stages.",
            TechnicalStack = "ASP.NET Core MVC, SQL Server, SignalR, Docker, Serilog, Bootstrap 5",
            ResearchAreaId = areas["Cyber Security"],
            Methodology = "Adopt secure-by-design architecture, iterative development, penetration testing, and controlled validation scenarios to assess alert quality and operational resilience.",
            TeamSize = "3 Members",
            StudentOwnerId = student2Id,
            Status = ProposalStatus.Matched,
            SubmittedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-5),
            IsMatched = true
        };
        matchedProposal.Keywords.Add(new ProposalKeyword { Keyword = "Privacy" });
        matchedProposal.Keywords.Add(new ProposalKeyword { Keyword = "Threat Intelligence" });
        matchedProposal.SupervisorInterests.Add(new SupervisorInterest { SupervisorId = supervisor2Id, CreatedAt = DateTime.UtcNow.AddDays(-10), IsConfirmed = true });
        matchedProposal.StatusHistory.Add(new ProposalStatusHistory { Status = ProposalStatus.Submitted, Note = "Proposal submitted by student group.", ChangedByUserId = student2Id, ChangedAt = DateTime.UtcNow.AddDays(-20) });
        matchedProposal.StatusHistory.Add(new ProposalStatusHistory { Status = ProposalStatus.UnderReview, Note = "Supervisor expressed interest and review started.", ChangedByUserId = supervisor2Id, ChangedAt = DateTime.UtcNow.AddDays(-10) });
        matchedProposal.StatusHistory.Add(new ProposalStatusHistory { Status = ProposalStatus.Matched, Note = "Match confirmed and identities revealed.", ChangedByUserId = supervisor2Id, ChangedAt = DateTime.UtcNow.AddDays(-5) });
        matchedProposal.Match = new Match
        {
            SupervisorId = supervisor2Id,
            ConfirmedAt = DateTime.UtcNow.AddDays(-5),
            RevealedAt = DateTime.UtcNow.AddDays(-5),
            CreatedByAdminOverride = false,
            Status = MatchStatus.Confirmed
        };

        var reviewProposal = new Proposal
        {
            Title = "Smart Laboratory Resource Scheduling with Edge-Connected Sensors",
            Abstract = "A proposal to create an IoT-enabled laboratory scheduling and occupancy intelligence platform that helps academic departments optimise space allocation and monitor equipment usage with near real-time edge data.",
            TechnicalStack = "ASP.NET Core MVC, SQL Server, MQTT, Raspberry Pi, REST APIs",
            ResearchAreaId = areas["Internet of Things"],
            Methodology = "Combine requirements engineering, rapid prototyping, device simulation, and iterative field validation with analytics-driven refinements.",
            TeamSize = "2 Members",
            StudentOwnerId = student3Id,
            Status = ProposalStatus.Submitted,
            SubmittedAt = DateTime.UtcNow.AddDays(-4),
            UpdatedAt = DateTime.UtcNow.AddDays(-4)
        };
        reviewProposal.Keywords.Add(new ProposalKeyword { Keyword = "Scheduling" });
        reviewProposal.Keywords.Add(new ProposalKeyword { Keyword = "Edge" });
        reviewProposal.SupervisorInterests.Add(new SupervisorInterest { SupervisorId = supervisor1Id, CreatedAt = DateTime.UtcNow.AddDays(-2) });
        reviewProposal.StatusHistory.Add(new ProposalStatusHistory { Status = ProposalStatus.Submitted, Note = "Proposal submitted by student group.", ChangedByUserId = student3Id, ChangedAt = DateTime.UtcNow.AddDays(-4) });

        context.Proposals.AddRange(pendingProposal, matchedProposal, reviewProposal);
    }
}
