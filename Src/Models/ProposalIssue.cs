namespace ProposalIQ.Web.Models;

public sealed class ProposalIssue
{
    public string Category { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Evidence { get; set; } = string.Empty;

    public string WhyItMatters { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;
}