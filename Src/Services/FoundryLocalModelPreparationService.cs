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

            var catalog = await FoundryLocalManager.Instance.GetCatalogAsync(stoppingToken);

            var model = await catalog.GetModelAsync(options.ModelAlias, stoppingToken)
                ?? throw new InvalidOperationException(
                    $"Foundry Local model '{options.ModelAlias}' was not found in the catalog.");

            logger.LogInformation("Starting download for Foundry Local model '{ModelAlias}'...", options.ModelAlias);
            state.SetDownloading(0, $"Downloading model '{options.ModelAlias}'...");

            var lastReportedPercent = -1;
            await model.DownloadAsync(progress =>
            {
                var percent = (int)progress;
                state.SetDownloading(percent, $"Downloading model '{options.ModelAlias}' ({percent}%)...");

                if (percent != lastReportedPercent && (percent % 10 == 0 || percent == 100))
                {
                    lastReportedPercent = percent;
                    logger.LogInformation("Downloading Foundry Local model '{ModelAlias}': {Percent}%", options.ModelAlias, percent);
                }
            }, stoppingToken);

            logger.LogInformation("Foundry Local model '{ModelAlias}' download complete. Loading model...", options.ModelAlias);
            state.SetLoading($"Loading model '{options.ModelAlias}' into memory...");
            await model.LoadAsync(stoppingToken);

            logger.LogInformation("Foundry Local model '{ModelAlias}' loaded successfully and ready for use.", options.ModelAlias);
            state.MarkReady(model);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Foundry Local model preparation was cancelled for '{ModelAlias}'.", options.ModelAlias);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to prepare the Foundry Local model '{ModelAlias}'.", options.ModelAlias);
            state.MarkFaulted(ex, $"Failed to prepare model '{options.ModelAlias}': {ex.Message}");
        }
    }
}
