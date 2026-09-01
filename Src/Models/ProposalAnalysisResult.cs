namespace ProposalIQ.Web.Models;

public sealed class ProposalAnalysisResult
{
    public string OverallRisk { get; set; } = "Unknown";

    public string ExecutiveSummary { get; set; } = string.Empty;

    public RiskSummary ScopeRisk { get; set; } = new();

    public RiskSummary CommercialRisk { get; set; } = new();

    public RiskSummary TimelineRisk { get; set; } = new();

    public RiskSummary DeliveryRisk { get; set; } = new();

    public List<ProposalIssue> Issues { get; set; } = [];
}