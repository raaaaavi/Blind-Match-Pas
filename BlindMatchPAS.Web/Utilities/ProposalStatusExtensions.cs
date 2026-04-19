namespace BlindMatchPAS.Web.Utilities;

public static class ProposalStatusExtensions
{
    public static string ToBadgeClass(this ProposalStatus status) => status switch
    {
        ProposalStatus.Draft => "badge-soft-secondary",
        ProposalStatus.Submitted => "badge-soft-primary",
        ProposalStatus.PendingReview => "badge-soft-warning",
        ProposalStatus.UnderReview => "badge-soft-info",
        ProposalStatus.Matched => "badge-soft-success",
        ProposalStatus.Withdrawn => "badge-soft-danger",
        _ => "badge-soft-secondary"
    };

    public static string ToDisplayText(this ProposalStatus status) => status switch
    {
        ProposalStatus.PendingReview => "Pending Review",
        ProposalStatus.UnderReview => "Under Review",
        _ => status.ToString()
    };
}
