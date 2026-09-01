namespace ProposalIQ.Web.Services;

public interface IProposalTextExtractor
{
    Task<string> ExtractTextAsync(IFormFile file, CancellationToken cancellationToken = default);
}