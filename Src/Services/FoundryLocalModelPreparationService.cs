using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging;
using ProposalIQ.Web.Configuration;
using FoundryConfiguration = Microsoft.AI.Foundry.Local.Configuration;

namespace ProposalIQ.Web.Services;

// Downloads and loads the configured Foundry Local model without blocking application startup.
public class FoundryLocalModelPreparationService(
    FoundryLocalOptions options,
    FoundryLocalModelState state,
    ILogger<FoundryLocalModelPreparationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await FoundryLocalManager.CreateAsync(
                new FoundryConfiguration { AppName = options.AppName },
                logger);

            var catalog = await FoundryLocalManager.Instance.GetCatalogAsync();

            var model = await catalog.GetModelAsync(options.ModelAlias)
                ?? throw new InvalidOperationException(
                    $"Foundry Local model '{options.ModelAlias}' was not found in the catalog.");

            await model.DownloadAsync();
            await model.LoadAsync();

            var chatClient = await model.GetChatClientAsync();

            state.MarkReady(chatClient);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to prepare the Foundry Local model '{ModelAlias}'.", options.ModelAlias);
            state.MarkFaulted(ex);
        }
    }
}
