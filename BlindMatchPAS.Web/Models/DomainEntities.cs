using Microsoft.AspNetCore.Identity;

namespace BlindMatchPAS.Web.Models.Enums
{
    public enum ProposalStatus
    {
        Draft = 1,
        Submitted = 2,
        PendingReview = 3,
        UnderReview = 4,
        Matched = 5,
        Withdrawn = 6
    }

    public enum MatchStatus
    {
        Confirmed = 1,
        Reassigned = 2
    }
}

namespace BlindMatchPAS.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(120)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? RegistrationNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public StudentProfile? StudentProfile { get; set; }
        public SupervisorProfile? SupervisorProfile { get; set; }
        public ICollection<Proposal> OwnedProposals { get; set; } = new List<Proposal>();
        public ICollection<SupervisorInterest> SupervisorInterests { get; set; } = new List<SupervisorInterest>();
        public ICollection<Match> SupervisorMatches { get; set; } = new List<Match>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    public class StudentProfile
    {
        public int Id { get; set; }

        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(40)]
        public string StudentIdentifier { get; set; } = string.Empty;

        [StringLength(120)]
        public string Programme { get; set; } = string.Empty;

        [StringLength(80)]
        public string GroupName { get; set; } = string.Empty;

        [StringLength(200)]
        public string TeamMemberNames { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
    }

    public class SupervisorProfile
    {
        public int Id { get; set; }

        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Department { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(80)]
        public string OfficeLocation { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
        public ICollection<SupervisorExpertise> ExpertiseAreas { get; set; } = new List<SupervisorExpertise>();
    }

    public class ResearchArea
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(280)]
        public string Description { get; set; } = string.Empty;

        [StringLength(30)]
        public string AccentColor { get; set; } = "#5b6cff";

        public bool IsActive { get; set; } = true;

        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public ICollection<SupervisorExpertise> SupervisorExpertise { get; set; } = new List<SupervisorExpertise>();
    }

    public class Proposal
    {
        public int Id { get; set; }

        [Required, StringLength(160, MinimumLength = 8)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000, MinimumLength = 80)]
        public string Abstract { get; set; } = string.Empty;

        [Required, StringLength(500, MinimumLength = 3)]
        public string TechnicalStack { get; set; } = string.Empty;

        [Required]
        public int ResearchAreaId { get; set; }

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
