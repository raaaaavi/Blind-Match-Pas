using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<SupervisorProfile> SupervisorProfiles => Set<SupervisorProfile>();
    public DbSet<ResearchArea> ResearchAreas => Set<ResearchArea>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalKeyword> ProposalKeywords => Set<ProposalKeyword>();
    public DbSet<SupervisorExpertise> SupervisorExpertise => Set<SupervisorExpertise>();
    public DbSet<SupervisorInterest> SupervisorInterests => Set<SupervisorInterest>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ProposalStatusHistory> ProposalStatusHistories => Set<ProposalStatusHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<StudentProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        builder.Entity<StudentProfile>()
            .HasOne(x => x.User)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey<StudentProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SupervisorProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        builder.Entity<SupervisorProfile>()
            .HasOne(x => x.User)
            .WithOne(x => x.SupervisorProfile)
            .HasForeignKey<SupervisorProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ResearchArea>()
            .HasIndex(x => x.Name)
            .IsUnique();

        builder.Entity<Proposal>()
            .Property(x => x.Status)
            .HasConversion<string>();

        builder.Entity<Proposal>()
            .HasOne(x => x.StudentOwner)
            .WithMany(x => x.OwnedProposals)
            .HasForeignKey(x => x.StudentOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProposalKeyword>()
            .HasIndex(x => new { x.ProposalId, x.Keyword })
            .IsUnique();

        builder.Entity<SupervisorExpertise>()
            .HasIndex(x => new { x.SupervisorId, x.ResearchAreaId })
            .IsUnique();

        builder.Entity<SupervisorInterest>()
            .HasIndex(x => new { x.ProposalId, x.SupervisorId })
            .IsUnique();

        builder.Entity<Match>()
            .Property(x => x.Status)
            .HasConversion<string>();

        builder.Entity<Match>()
            .HasIndex(x => x.ProposalId)
            .IsUnique();

        builder.Entity<Match>()
            .HasOne(x => x.Proposal)
            .WithOne(x => x.Match)
            .HasForeignKey<Match>(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Match>()
            .HasOne(x => x.Supervisor)
            .WithMany(x => x.SupervisorMatches)
            .HasForeignKey(x => x.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProposalStatusHistory>()
            .Property(x => x.Status)
            .HasConversion<string>();

        builder.Entity<AuditLog>()
            .HasOne(x => x.User)
            .WithMany(x => x.AuditLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Notification>()
            .HasOne(x => x.User)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
