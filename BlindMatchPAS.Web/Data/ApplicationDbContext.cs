using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Data;

/// <summary>
/// ApplicationDbContext - Entity Framework Core Database Context
/// 
/// Extends IdentityDbContext to include ASP.NET Core Identity tables (Users, Roles, Claims)
/// plus custom entities for the Blind-Match PAS system.
/// 
/// Key Responsibilities:
/// - Define all DbSets for domain entities
/// - Configure entity relationships and constraints
/// - Handle cascade delete rules
/// - Create unique indexes for data integrity
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    // ===== USER PROFILE ENTITIES =====
    /// <summary>Student-specific profile information</summary>
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    
    /// <summary>Supervisor-specific profile information and expertise</summary>
    public DbSet<SupervisorProfile> SupervisorProfiles => Set<SupervisorProfile>();

    // ===== ACADEMIC STRUCTURE ENTITIES =====
    /// <summary>Research domains/areas for categorizing proposals and expertise</summary>
    public DbSet<ResearchArea> ResearchAreas => Set<ResearchArea>();

    // ===== PROPOSAL MANAGEMENT ENTITIES =====
    /// <summary>Student project proposals - core entity of the system</summary>
    public DbSet<Proposal> Proposals => Set<Proposal>();
    
    /// <summary>Keywords associated with proposals for searching/filtering</summary>
    public DbSet<ProposalKeyword> ProposalKeywords => Set<ProposalKeyword>();

    // ===== SUPERVISOR EXPERTISE AND INTEREST ENTITIES =====
    /// <summary>Tracks which research areas supervisors have expertise in</summary>
    public DbSet<SupervisorExpertise> SupervisorExpertise => Set<SupervisorExpertise>();
    
    /// <summary>Tracks supervisor interest in specific proposals (before match confirmation)</summary>
    public DbSet<SupervisorInterest> SupervisorInterests => Set<SupervisorInterest>();

    // ===== MATCHING ENTITIES =====
    /// <summary>Confirmed matches between supervisors and proposals</summary>
    public DbSet<Match> Matches => Set<Match>();

    // ===== AUDIT AND HISTORY ENTITIES =====
    /// <summary>System audit logs for tracking all important actions</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    /// <summary>Timeline history of proposal status changes</summary>
    public DbSet<ProposalStatusHistory> ProposalStatusHistories => Set<ProposalStatusHistory>();
    
    /// <summary>Notifications for users about important system events</summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>
    /// Configures entity relationships, constraints, unique indexes, and cascade delete behavior.
    /// This method ensures data integrity and defines the database schema structure.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===== STUDENT PROFILE CONFIGURATION =====
        // Each user can have at most one student profile
        builder.Entity<StudentProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        // One-to-one relationship: User -> StudentProfile (cascade delete)
        builder.Entity<StudentProfile>()
            .HasOne(x => x.User)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey<StudentProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== SUPERVISOR PROFILE CONFIGURATION =====
        // Each user can have at most one supervisor profile
        builder.Entity<SupervisorProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        // One-to-one relationship: User -> SupervisorProfile (cascade delete)
        builder.Entity<SupervisorProfile>()
            .HasOne(x => x.User)
            .WithOne(x => x.SupervisorProfile)
            .HasForeignKey<SupervisorProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== RESEARCH AREA CONFIGURATION =====
        // Research area names must be unique (no duplicate categories)
        builder.Entity<ResearchArea>()
            .HasIndex(x => x.Name)
            .IsUnique();

        // ===== PROPOSAL CONFIGURATION =====
        // Store ProposalStatus enum as string in database (e.g., "Pending", "Matched")
        builder.Entity<Proposal>()
            .Property(x => x.Status)
            .HasConversion<string>();

        // Many-to-one relationship: Proposal -> StudentOwner (restrict delete on referenced student)
        // This prevents accidental data loss if a student record is somehow deleted
        builder.Entity<Proposal>()
            .HasOne(x => x.StudentOwner)
            .WithMany(x => x.OwnedProposals)
            .HasForeignKey(x => x.StudentOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== PROPOSAL KEYWORD CONFIGURATION =====
        // Prevent duplicate keywords for the same proposal
        builder.Entity<ProposalKeyword>()
            .HasIndex(x => new { x.ProposalId, x.Keyword })
            .IsUnique();

        // ===== SUPERVISOR EXPERTISE CONFIGURATION =====
        // Each supervisor can express expertise in multiple research areas, but only once per area
        builder.Entity<SupervisorExpertise>()
            .HasIndex(x => new { x.SupervisorId, x.ResearchAreaId })
            .IsUnique();

        // ===== SUPERVISOR INTEREST CONFIGURATION =====
        // Each supervisor can express interest in a proposal only once (prevent duplicate interests)
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
