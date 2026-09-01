namespace ProposalIQ.Web.Models;

public sealed class RiskSummary
{
    public string Level { get; set; } = "Unknown";

    public string Summary { get; set; } = string.Empty;

    public int IssueCount { get; set; }
}
