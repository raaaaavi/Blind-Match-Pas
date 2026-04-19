namespace BlindMatchPAS.Web.ViewModels.Shared;

public class StatusTimelineItemViewModel
{
    public string Status { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}

public class ResearchAreaOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccentColor { get; set; } = string.Empty;
}
