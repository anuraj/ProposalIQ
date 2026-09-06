using ProposalIQ.Web.Configuration;

namespace ProposalIQ.Web.Services;

public interface IAiConfigurationService
{
    AiProviderOptions GetOptions();

    Task UpdateOptionsAsync(AiProviderOptions options, CancellationToken cancellationToken = default);
}
