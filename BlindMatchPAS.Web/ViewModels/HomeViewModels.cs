namespace BlindMatchPAS.Web.ViewModels.Home;

public class LandingPageViewModel
{
    public int StudentCount { get; set; }
    public int SupervisorCount { get; set; }
    public int ProposalCount { get; set; }
    public int MatchCount { get; set; }
    public List<ResearchAreaOptionViewModel> ResearchAreas { get; set; } = [];
}
