namespace ProposalIQ.Web.Configuration;

public enum AiProvider
{
    OpenRouter,
    OpenAI,
    AzureOpenAI,
    Local,
    FoundryLocal
}

public class AiProviderOptions
{
    public AiProvider Provider { get; set; } = AiProvider.FoundryLocal;

    public OpenRouterOptions OpenRouter { get; set; } = new();

    public OpenAiOptions OpenAI { get; set; } = new();

    public AzureOpenAiOptions AzureOpenAI { get; set; } = new();

    public LocalOptions Local { get; set; } = new();

    public FoundryLocalOptions FoundryLocal { get; set; } = new();
}

public class OpenRouterOptions
{
    public string Model { get; set; } = "openrouter/auto";

    public string Endpoint { get; set; } = "https://openrouter.ai/api/v1/";

    public string ApiKey { get; set; } = string.Empty;
}

public class OpenAiOptions
{
    public string Model { get; set; } = "gpt-4o-mini";

    public string ApiKey { get; set; } = string.Empty;
}

public class AzureOpenAiOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string Deployment { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}

public class LocalOptions
{
    public string Model { get; set; } = string.Empty;

    public string Endpoint { get; set; } = "http://localhost:11434/v1/";
}

public class FoundryLocalOptions
{
    public string AppName { get; set; } = "ProposalIQ";

    public string ModelAlias { get; set; } = "qwen2.5-0.5b";
}
