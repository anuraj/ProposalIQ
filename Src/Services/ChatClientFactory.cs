using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using ProposalIQ.Web.Configuration;

namespace ProposalIQ.Web.Services;

public static class ChatClientFactory
{
    public static IChatClient Create(AiProviderOptions options)
    {
        return options.Provider switch
        {
            AiProvider.OpenRouter => CreateOpenAiCompatible(
                "Ai:OpenRouter:ApiKey", options.OpenRouter.ApiKey,
                "Ai:OpenRouter:Model", options.OpenRouter.Model,
                "Ai:OpenRouter:Endpoint", options.OpenRouter.Endpoint,
                allowEmptyApiKey: false),
            AiProvider.OpenAI => CreateOpenAi(options.OpenAI),
            AiProvider.AzureOpenAI => CreateAzureOpenAi(options.AzureOpenAI),
            AiProvider.Local => CreateOpenAiCompatible(
                "Ai:Local:ApiKey", string.Empty,
                "Ai:Local:Model", options.Local.Model,
                "Ai:Local:Endpoint", options.Local.Endpoint,
                allowEmptyApiKey: true),
            _ => throw new InvalidOperationException(
                $"Unsupported AI provider '{options.Provider}'. Supported values: {string.Join(", ", Enum.GetNames<AiProvider>())}.")
        };
    }

    private static IChatClient CreateOpenAi(OpenAiOptions options)
    {
        RequireNonBlank("Ai:OpenAI:ApiKey", options.ApiKey);
        RequireNonBlank("Ai:OpenAI:Model", options.Model);

        return new OpenAIClient(new ApiKeyCredential(options.ApiKey))
            .GetChatClient(options.Model)
            .AsIChatClient();
    }

    private static IChatClient CreateAzureOpenAi(AzureOpenAiOptions options)
    {
        RequireNonBlank("Ai:AzureOpenAI:Endpoint", options.Endpoint);
        RequireNonBlank("Ai:AzureOpenAI:Deployment", options.Deployment);
        RequireNonBlank("Ai:AzureOpenAI:ApiKey", options.ApiKey);

        return new AzureOpenAIClient(new Uri(options.Endpoint), new ApiKeyCredential(options.ApiKey))
            .GetChatClient(options.Deployment)
            .AsIChatClient();
    }

    private static IChatClient CreateOpenAiCompatible(
        string apiKeySettingName, string apiKey,
        string modelSettingName, string model,
        string endpointSettingName, string endpoint,
        bool allowEmptyApiKey)
    {
        if (!allowEmptyApiKey)
        {
            RequireNonBlank(apiKeySettingName, apiKey);
        }

        RequireNonBlank(modelSettingName, model);
        RequireNonBlank(endpointSettingName, endpoint);

        return new OpenAIClient(
                new ApiKeyCredential(string.IsNullOrWhiteSpace(apiKey) ? "not-required" : apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
            .GetChatClient(model)
            .AsIChatClient();
    }

    private static void RequireNonBlank(string settingName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration setting '{settingName}' is required but was not provided.");
        }
    }
}
