namespace ProposalIQ.Web.Models;

public class AnalyzeProposalRequest
{
    public IFormFile? ProposalFile { get; set; }

    public decimal? ProjectValue { get; set; }

    public decimal? HourlyRate { get; set; }

    public string? AdditionalContext { get; set; }
}