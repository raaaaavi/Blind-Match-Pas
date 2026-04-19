using Microsoft.AspNetCore.Identity;

namespace BlindMatchPAS.Web.Models.Enums
{
    /// <summary>
    /// ProposalStatus Enum - Tracks the lifecycle state of a proposal
    /// 
    /// States:
    /// - Draft: Initial creation, not yet submitted for review
    /// - Submitted: Student submitted, awaiting admin approval
    /// - PendingReview: Admin approved, visible to supervisors (anonymous)
    /// - UnderReview: Supervisor has expressed interest
    /// - Matched: Confirmed match between student and supervisor
    /// - Withdrawn: Student withdrew proposal (cannot be undone)
    /// </summary>
    public enum ProposalStatus
    {
        Draft = 1,
        Submitted = 2,
        PendingReview = 3,
        UnderReview = 4,
        Matched = 5,
        Withdrawn = 6
    }

    /// <summary>
    /// MatchStatus Enum - Tracks match confirmation state
    /// 
    /// States:
    /// - Confirmed: Initial match confirmed by supervisor
    /// - Reassigned: Admin override/reassignment action
    /// </summary>
    public enum MatchStatus
    {
        Confirmed = 1,
        Reassigned = 2
    }
}

namespace BlindMatchPAS.Web.Models.Entities
{
    /// <summary>
    /// ApplicationUser - Extended ASP.NET Core Identity user
    /// 
    /// Inherits from IdentityUser for authentication and authorization.
    /// Adds academic-specific properties and navigation to related entities.
    /// Can have StudentProfile, SupervisorProfile, or neither (admin role).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>Full display name (used in UI)</summary>
        [StringLength(120)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Student ID or employee number for registration</summary>
        [StringLength(50)]
        public string? RegistrationNumber { get; set; }

        /// <summary>Soft-delete flag: true = account active, false = disabled</summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        /// <summary>One-to-one link to StudentProfile (if user is student)</summary>
        public StudentProfile? StudentProfile { get; set; }
        
        /// <summary>One-to-one link to SupervisorProfile (if user is supervisor)</summary>
        public SupervisorProfile? SupervisorProfile { get; set; }
        
        /// <summary>All proposals owned by this student</summary>
        public ICollection<Proposal> OwnedProposals { get; set; } = new List<Proposal>();
        
        /// <summary>All interests expressed by this supervisor</summary>
        public ICollection<SupervisorInterest> SupervisorInterests { get; set; } = new List<SupervisorInterest>();
        
        /// <summary>All matches where this user is supervisor</summary>
        public ICollection<Match> SupervisorMatches { get; set; } = new List<Match>();
        
        /// <summary>Audit log entries created by this user</summary>
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        
        /// <summary>Notifications for this user</summary>
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    /// <summary>
    /// StudentProfile - Student-specific academic information
    /// 
    /// One-to-one relationship with ApplicationUser.
    /// Stores program, group, and team information.
    /// </summary>
    public class StudentProfile
    {
        public int Id { get; set; }

        /// <summary>Foreign key to ApplicationUser</summary>
        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Unique student identifier (e.g., "SID123")</summary>
        [Required, StringLength(40)]
        public string StudentIdentifier { get; set; } = string.Empty;

        /// <summary>Degree programme/major</summary>
        [StringLength(120)]
        public string Programme { get; set; } = string.Empty;

        /// <summary>Project group/cohort name</summary>
        [StringLength(80)]
        public string GroupName { get; set; } = string.Empty;

        /// <summary>Team member names (if group project)</summary>
        [StringLength(200)]
        public string TeamMemberNames { get; set; } = string.Empty;

        /// <summary>Navigation: reference to ApplicationUser</summary>
        public ApplicationUser? User { get; set; }
    }

    /// <summary>
    /// SupervisorProfile - Supervisor-specific academic information
    /// 
    /// One-to-one relationship with ApplicationUser.
    /// Stores department, specialization, and expertise areas.
    /// </summary>
    public class SupervisorProfile
    {
        public int Id { get; set; }

        /// <summary>Foreign key to ApplicationUser</summary>
        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Academic department</summary>
        [Required, StringLength(120)]
        public string Department { get; set; } = string.Empty;

        /// <summary>Primary specialization/research focus</summary>
        [Required, StringLength(150)]
        public string Specialization { get; set; } = string.Empty;

        /// <summary>Office location</summary>
        [StringLength(80)]
        public string OfficeLocation { get; set; } = string.Empty;

        /// <summary>Navigation: reference to ApplicationUser</summary>
        public ApplicationUser? User { get; set; }
        
        /// <summary>Navigation: expertise areas this supervisor claims</summary>
        public ICollection<SupervisorExpertise> ExpertiseAreas { get; set; } = new List<SupervisorExpertise>();
    }

    /// <summary>
    /// ResearchArea - Academic domain/category for proposals
    /// 
    /// Organizes proposals and supervisor expertise by research domain.
    /// Can be deactivated to prevent new proposals in that area.
    /// </summary>
    public class ResearchArea
    {
        public int Id { get; set; }

        /// <summary>Unique research area name (e.g., "Software Engineering")</summary>
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Description of research area</summary>
        [StringLength(280)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Accent color for UI (hex code)</summary>
        [StringLength(30)]
        public string AccentColor { get; set; } = "#5b6cff";

        /// <summary>Flag: true = active (available for selection), false = inactive</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Navigation: all proposals in this research area</summary>
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        
        /// <summary>Navigation: supervisors claiming expertise in this area</summary>
        public ICollection<SupervisorExpertise> SupervisorExpertise { get; set; } = new List<SupervisorExpertise>();
    }

    /// <summary>
    /// Proposal - Student project proposal (core entity)
    /// 
    /// Lifecycle:
    /// 1. Student creates proposal (status = Draft)
    /// 2. Student submits (status = Submitted)
    /// 3. Admin approves (status = PendingReview)
    /// 4. Supervisors browse anonymously, express interest
    /// 5. Supervisor confirms match (status = Matched, identity revealed)
    /// OR Student can withdraw (status = Withdrawn)
    /// </summary>
    public class Proposal
    {
        public int Id { get; set; }

        /// <summary>Proposal title (min 8, max 160 chars)</summary>
        [Required, StringLength(160, MinimumLength = 8)]
        public string Title { get; set; } = string.Empty;

        /// <summary>Project abstract/summary (min 80, max 2000 chars)</summary>
        [Required, StringLength(2000, MinimumLength = 80)]
        public string Abstract { get; set; } = string.Empty;

        /// <summary>Technology stack used (min 3, max 500 chars)</summary>
        [Required, StringLength(500, MinimumLength = 3)]
        public string TechnicalStack { get; set; } = string.Empty;

        /// <summary>Foreign key to ResearchArea</summary>
        [Required]
        public int ResearchAreaId { get; set; }

        /// <summary>Methodology and approach (min 40, max 1500 chars)</summary>
        [Required, StringLength(1500, MinimumLength = 40)]
        public string Methodology { get; set; } = string.Empty;

        [StringLength(120)]
        public string TeamSize { get; set; } = string.Empty;

        [Required, StringLength(450)]
        public string StudentOwnerId { get; set; } = string.Empty;

        public ProposalStatus Status { get; set; } = ProposalStatus.Draft;
        public DateTime? SubmittedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsWithdrawn { get; set; }
        public bool IsMatched { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public ResearchArea? ResearchArea { get; set; }
        public ApplicationUser? StudentOwner { get; set; }
        public ICollection<ProposalKeyword> Keywords { get; set; } = new List<ProposalKeyword>();
        public ICollection<SupervisorInterest> SupervisorInterests { get; set; } = new List<SupervisorInterest>();
        public Match? Match { get; set; }
        public ICollection<ProposalStatusHistory> StatusHistory { get; set; } = new List<ProposalStatusHistory>();
    }

    public class ProposalKeyword
    {
        public int Id { get; set; }
        public int ProposalId { get; set; }

        [Required, StringLength(40)]
        public string Keyword { get; set; } = string.Empty;

        public Proposal? Proposal { get; set; }
    }

    public class SupervisorExpertise
    {
        public int Id { get; set; }

        [Required, StringLength(450)]
        public string SupervisorId { get; set; } = string.Empty;

        public int ResearchAreaId { get; set; }

        public ApplicationUser? Supervisor { get; set; }
        public ResearchArea? ResearchArea { get; set; }
    }

    public class SupervisorInterest
    {
        public int Id { get; set; }
        public int ProposalId { get; set; }

        [Required, StringLength(450)]
        public string SupervisorId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsConfirmed { get; set; }

        public Proposal? Proposal { get; set; }
        public ApplicationUser? Supervisor { get; set; }
    }

    public class Match
    {
        public int Id { get; set; }
        public int ProposalId { get; set; }

        [Required, StringLength(450)]
        public string SupervisorId { get; set; } = string.Empty;

        public DateTime ConfirmedAt { get; set; } = DateTime.UtcNow;
        public DateTime RevealedAt { get; set; } = DateTime.UtcNow;
        public MatchStatus Status { get; set; } = MatchStatus.Confirmed;
        public bool CreatedByAdminOverride { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public Proposal? Proposal { get; set; }
        public ApplicationUser? Supervisor { get; set; }
    }

    public class ProposalStatusHistory
    {
        public int Id { get; set; }
        public int ProposalId { get; set; }
        public ProposalStatus Status { get; set; }

        [StringLength(300)]
        public string Note { get; set; } = string.Empty;

        [StringLength(450)]
        public string? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public Proposal? Proposal { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Action { get; set; } = string.Empty;

        [StringLength(80)]
        public string EntityName { get; set; } = string.Empty;

        [StringLength(120)]
        public string EntityId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(500)]
        public string Details { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ApplicationUser? User { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }

        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(140)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(400)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ApplicationUser? User { get; set; }
    }
}
