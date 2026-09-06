using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using ProposalIQ.Web.Configuration;

namespace ProposalIQ.Web.Services;

public class DynamicChatClient : IChatClient
{
    private readonly IAiConfigurationService _configService;
    private readonly FoundryLocalModelState _foundryLocalState;
    private readonly object _gate = new();
    private AiProviderOptions? _lastOptions;
    private IChatClient? _cachedClient;

    public DynamicChatClient(
        IAiConfigurationService configService,
        FoundryLocalModelState foundryLocalState)
    {
        _configService = configService;
        _foundryLocalState = foundryLocalState;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var client = GetCurrentChatClient();
        return await client.GetResponseAsync(messages, options, cancellationToken);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = GetCurrentChatClient();
        await foreach (var update in client.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            yield return update;
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey is null && serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        var inner = GetCurrentChatClient();
        return inner.GetService(serviceType, serviceKey);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _cachedClient?.Dispose();
            _cachedClient = null;
        }
    }

    private IChatClient GetCurrentChatClient()
    {
        var currentOptions = _configService.GetOptions();

        lock (_gate)
        {
            if (_cachedClient != null && AreOptionsEqual(_lastOptions, currentOptions))
            {
                return _cachedClient;
            }

            _cachedClient?.Dispose();
            _lastOptions = currentOptions;
            _cachedClient = ChatClientFactory.Create(currentOptions, _foundryLocalState);
            return _cachedClient;
        }
    }

    private static bool AreOptionsEqual(AiProviderOptions? a, AiProviderOptions? b)
    {
        if (a == null || b == null) return false;
        if (a.Provider != b.Provider) return false;

        return a.Provider switch
        {
            AiProvider.OpenRouter =>
                a.OpenRouter.Model == b.OpenRouter.Model &&
                a.OpenRouter.Endpoint == b.OpenRouter.Endpoint &&
                a.OpenRouter.ApiKey == b.OpenRouter.ApiKey,
            AiProvider.OpenAI =>
                a.OpenAI.Model == b.OpenAI.Model &&
                a.OpenAI.ApiKey == b.OpenAI.ApiKey,
            AiProvider.AzureOpenAI =>
                a.AzureOpenAI.Endpoint == b.AzureOpenAI.Endpoint &&
                a.AzureOpenAI.Deployment == b.AzureOpenAI.Deployment &&
                a.AzureOpenAI.ApiKey == b.AzureOpenAI.ApiKey,
            AiProvider.Local =>
                a.Local.Model == b.Local.Model &&
                a.Local.Endpoint == b.Local.Endpoint,
            AiProvider.FoundryLocal =>
                a.FoundryLocal.ModelAlias == b.FoundryLocal.ModelAlias &&
                a.FoundryLocal.AppName == b.FoundryLocal.AppName,
            _ => false
        };
    }
}
