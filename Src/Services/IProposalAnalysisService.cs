using ProposalIQ.Web.Models;

namespace ProposalIQ.Web.Services;

public interface IProposalAnalysisService
{
    Task<ProposalAnalysisResult> AnalyzeAsync(string proposalText, AnalyzeProposalRequest request, CancellationToken cancellationToken = default);
}