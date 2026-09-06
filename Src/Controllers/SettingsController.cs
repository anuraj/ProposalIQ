using Microsoft.AspNetCore.Mvc;
using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Models;
using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Controllers;

public class SettingsController(
    IAiConfigurationService configService,
    IFoundryLocalModelManager modelManager) : Controller
{
    private readonly IAiConfigurationService _configService = configService;
    private readonly IFoundryLocalModelManager _modelManager = modelManager;

    [HttpGet]
    public IActionResult Index()
    {
        var options = _configService.GetOptions();
        var model = ToViewModel(options);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel model, CancellationToken cancellationToken)
    {
        var existingOptions = _configService.GetOptions();

        // Clear automatic model binder validation errors for inactive providers
        ModelState.Clear();

        // Preserve existing API keys if left blank on edit
        var openRouterKey = string.IsNullOrWhiteSpace(model.OpenRouter.ApiKey)
            ? existingOptions.OpenRouter.ApiKey
            : model.OpenRouter.ApiKey.Trim();

        var openAiKey = string.IsNullOrWhiteSpace(model.OpenAI.ApiKey)
            ? existingOptions.OpenAI.ApiKey
            : model.OpenAI.ApiKey.Trim();

        var azureOpenAiKey = string.IsNullOrWhiteSpace(model.AzureOpenAI.ApiKey)
            ? existingOptions.AzureOpenAI.ApiKey
            : model.AzureOpenAI.ApiKey.Trim();

        ValidateProviderSettings(model.ActiveProvider, model, openRouterKey, openAiKey, azureOpenAiKey);

        if (!ModelState.IsValid)
        {
            model.HasOpenRouterApiKey = !string.IsNullOrEmpty(existingOptions.OpenRouter.ApiKey);
            model.HasOpenAiApiKey = !string.IsNullOrEmpty(existingOptions.OpenAI.ApiKey);
            model.HasAzureOpenAiApiKey = !string.IsNullOrEmpty(existingOptions.AzureOpenAI.ApiKey);
            return View(model);
        }

        var newOptions = new AiProviderOptions
        {
            Provider = model.ActiveProvider,
            OpenRouter = new OpenRouterOptions
            {
                Model = string.IsNullOrWhiteSpace(model.OpenRouter.Model) ? existingOptions.OpenRouter.Model : model.OpenRouter.Model.Trim(),
                Endpoint = string.IsNullOrWhiteSpace(model.OpenRouter.Endpoint) ? existingOptions.OpenRouter.Endpoint : model.OpenRouter.Endpoint.Trim(),
                ApiKey = openRouterKey
            },
            OpenAI = new OpenAiOptions
            {
                Model = string.IsNullOrWhiteSpace(model.OpenAI.Model) ? existingOptions.OpenAI.Model : model.OpenAI.Model.Trim(),
                ApiKey = openAiKey
            },
            AzureOpenAI = new AzureOpenAiOptions
            {
                Endpoint = string.IsNullOrWhiteSpace(model.AzureOpenAI.Endpoint) ? existingOptions.AzureOpenAI.Endpoint : model.AzureOpenAI.Endpoint.Trim(),
                Deployment = string.IsNullOrWhiteSpace(model.AzureOpenAI.Deployment) ? existingOptions.AzureOpenAI.Deployment : model.AzureOpenAI.Deployment.Trim(),
                ApiKey = azureOpenAiKey
            },
            Local = new LocalOptions
            {
                Model = string.IsNullOrWhiteSpace(model.Local.Model) ? existingOptions.Local.Model : model.Local.Model.Trim(),
                Endpoint = string.IsNullOrWhiteSpace(model.Local.Endpoint) ? existingOptions.Local.Endpoint : model.Local.Endpoint.Trim()
            },
            FoundryLocal = new FoundryLocalOptions
            {
                AppName = string.IsNullOrWhiteSpace(model.FoundryLocal.AppName) ? existingOptions.FoundryLocal.AppName : model.FoundryLocal.AppName.Trim(),
                ModelAlias = string.IsNullOrWhiteSpace(model.FoundryLocal.ModelAlias) ? existingOptions.FoundryLocal.ModelAlias : model.FoundryLocal.ModelAlias.Trim()
            }
        };

        await _configService.UpdateOptionsAsync(newOptions, cancellationToken);

        if (newOptions.Provider == AiProvider.FoundryLocal)
        {
            _ = Task.Run(() => _modelManager.EnsureModelPreparedAsync(newOptions.FoundryLocal, CancellationToken.None));
        }

        var updatedModel = ToViewModel(newOptions);
        updatedModel.SuccessMessage = $"AI Provider configuration saved successfully. Active provider is now '{newOptions.Provider}'.";
        return View(updatedModel);
    }

    private void ValidateProviderSettings(
        AiProvider provider,
        SettingsViewModel model,
        string openRouterKey,
        string openAiKey,
        string azureOpenAiKey)
    {
        switch (provider)
        {
            case AiProvider.OpenRouter:
                if (string.IsNullOrWhiteSpace(model.OpenRouter.Model))
                    ModelState.AddModelError("OpenRouter.Model", "OpenRouter Model is required.");
                if (string.IsNullOrWhiteSpace(model.OpenRouter.Endpoint))
                    ModelState.AddModelError("OpenRouter.Endpoint", "OpenRouter Endpoint is required.");
                if (string.IsNullOrWhiteSpace(openRouterKey))
                    ModelState.AddModelError("OpenRouter.ApiKey", "OpenRouter API Key is required.");
                break;

            case AiProvider.OpenAI:
                if (string.IsNullOrWhiteSpace(model.OpenAI.Model))
                    ModelState.AddModelError("OpenAI.Model", "OpenAI Model is required.");
                if (string.IsNullOrWhiteSpace(openAiKey))
                    ModelState.AddModelError("OpenAI.ApiKey", "OpenAI API Key is required.");
                break;

            case AiProvider.AzureOpenAI:
                if (string.IsNullOrWhiteSpace(model.AzureOpenAI.Endpoint))
                    ModelState.AddModelError("AzureOpenAI.Endpoint", "Azure OpenAI Endpoint is required.");
                if (string.IsNullOrWhiteSpace(model.AzureOpenAI.Deployment))
                    ModelState.AddModelError("AzureOpenAI.Deployment", "Azure OpenAI Deployment is required.");
                if (string.IsNullOrWhiteSpace(azureOpenAiKey))
                    ModelState.AddModelError("AzureOpenAI.ApiKey", "Azure OpenAI API Key is required.");
                break;

            case AiProvider.Local:
                if (string.IsNullOrWhiteSpace(model.Local.Model))
                    ModelState.AddModelError("Local.Model", "Local Model is required.");
                if (string.IsNullOrWhiteSpace(model.Local.Endpoint))
                    ModelState.AddModelError("Local.Endpoint", "Local Endpoint is required.");
                break;

            case AiProvider.FoundryLocal:
                if (string.IsNullOrWhiteSpace(model.FoundryLocal.ModelAlias))
                    ModelState.AddModelError("FoundryLocal.ModelAlias", "Foundry Local Model Alias is required.");
                break;
        }
    }

    private static SettingsViewModel ToViewModel(AiProviderOptions options)
    {
        return new SettingsViewModel
        {
            ActiveProvider = options.Provider,
            OpenRouter = new OpenRouterOptions
            {
                Model = options.OpenRouter.Model,
                Endpoint = options.OpenRouter.Endpoint,
                ApiKey = string.Empty
            },
            OpenAI = new OpenAiOptions
            {
                Model = options.OpenAI.Model,
                ApiKey = string.Empty
            },
            AzureOpenAI = new AzureOpenAiOptions
            {
                Endpoint = options.AzureOpenAI.Endpoint,
                Deployment = options.AzureOpenAI.Deployment,
                ApiKey = string.Empty
            },
            Local = new LocalOptions
            {
                Model = options.Local.Model,
                Endpoint = options.Local.Endpoint
            },
            FoundryLocal = new FoundryLocalOptions
            {
                AppName = options.FoundryLocal.AppName,
                ModelAlias = options.FoundryLocal.ModelAlias
            },
            HasOpenRouterApiKey = !string.IsNullOrWhiteSpace(options.OpenRouter.ApiKey),
            HasOpenAiApiKey = !string.IsNullOrWhiteSpace(options.OpenAI.ApiKey),
            HasAzureOpenAiApiKey = !string.IsNullOrWhiteSpace(options.AzureOpenAI.ApiKey)
        };
    }
}
