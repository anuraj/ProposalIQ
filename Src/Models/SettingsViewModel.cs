using ProposalIQ.Web.Configuration;

namespace ProposalIQ.Web.Models;

public class SettingsViewModel
{
    public AiProvider ActiveProvider { get; set; } = AiProvider.FoundryLocal;

    public OpenRouterOptions OpenRouter { get; set; } = new();

    public OpenAiOptions OpenAI { get; set; } = new();

    public AzureOpenAiOptions AzureOpenAI { get; set; } = new();

    public LocalOptions Local { get; set; } = new();

    public FoundryLocalOptions FoundryLocal { get; set; } = new();

    public bool HasOpenRouterApiKey { get; set; }

    public bool HasOpenAiApiKey { get; set; }

    public bool HasAzureOpenAiApiKey { get; set; }

    public string? SuccessMessage { get; set; }
}
