using System.Runtime.CompilerServices;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using BetalgoChatMessage = Betalgo.Ranul.OpenAI.ObjectModels.RequestModels.ChatMessage;

namespace ProposalIQ.Web.Services;

// Bridges the Foundry Local SDK's chat client into Microsoft.Extensions.AI.IChatClient.
public class FoundryLocalChatClient(FoundryLocalModelState state) : IChatClient
{
    private readonly FoundryLocalModelState _state = state;

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var chatClient = GetReadyChatClientOrThrow();
        var betalgoMessages = messages.Select(ToBetalgoMessage).ToList();

        var completion = await chatClient.CompleteChatAsync(betalgoMessages, cancellationToken);

        var content = completion.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, content));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatClient = GetReadyChatClientOrThrow();
        var betalgoMessages = messages.Select(ToBetalgoMessage).ToList();

        await foreach (var chunk in chatClient.CompleteChatStreamingAsync(betalgoMessages, cancellationToken))
        {
            var delta = chunk.Choices?.FirstOrDefault()?.Delta?.Content
                ?? chunk.Choices?.FirstOrDefault()?.Message?.Content;

            if (!string.IsNullOrEmpty(delta))
            {
                yield return new ChatResponseUpdate(ChatRole.Assistant, delta);
            }
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;
    }

    public void Dispose()
    {
    }

    private OpenAIChatClient GetReadyChatClientOrThrow()
    {
        var fault = _state.Fault;

        if (fault != null)
        {
            throw new InvalidOperationException(
                "The Foundry Local model failed to prepare. See the application logs for details.",
                fault);
        }

        return _state.TryGetChatClient()
            ?? throw new InvalidOperationException(
                "The Foundry Local model is still preparing. Please try again shortly.");
    }

    private static BetalgoChatMessage ToBetalgoMessage(ChatMessage message)
    {
        return new BetalgoChatMessage
        {
            Role = message.Role.Value,
            Content = message.Text
        };
    }
}
