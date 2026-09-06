namespace ProposalIQ.Web.Services;

// Downloads and loads the configured Foundry Local model without blocking application startup.
public class FoundryLocalModelPreparationService(
    IAiConfigurationService configService,
    IFoundryLocalModelManager modelManager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = configService.GetOptions();
        if (options.Provider == Configuration.AiProvider.FoundryLocal)
        {
            await modelManager.EnsureModelPreparedAsync(options.FoundryLocal, stoppingToken);
        }
    }
}
