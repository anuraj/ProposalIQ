using ProposalIQ.Web.Models;

namespace ProposalIQ.Web.Services;

public interface IProposalAnalysisCache
{
    string ComputeCacheKey(string proposalText, AnalyzeProposalRequest request);

    Task<ProposalAnalysisResult?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task SetAsync(string cacheKey, ProposalAnalysisResult result, CancellationToken cancellationToken = default);
}
