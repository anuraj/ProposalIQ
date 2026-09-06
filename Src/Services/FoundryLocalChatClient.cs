using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;

namespace ProposalIQ.Web.Services;

// Bridges the Foundry Local SDK's chat session into Microsoft.Extensions.AI.IChatClient.
public class FoundryLocalChatClient(FoundryLocalModelState state) : IChatClient
{
    private readonly FoundryLocalModelState _state = state;

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var session = GetReadyChatSessionOrThrow();
        using var request = CreateRequest(messages, options);

        using var response = await session.ProcessRequestAsync(request, cancellationToken);
        var content = ExtractResponseText(response);

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, content));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var session = GetReadyChatSessionOrThrow();
        using var request = CreateRequest(messages, options);

        session.SetStreaming(true);
        var stream = session.ProcessStreamingRequestAsync(request, cancellationToken);

        await using (stream)
        {
            await foreach (var item in stream.WithCancellation(cancellationToken))
            {
                if (item is TextItem textItem && !string.IsNullOrEmpty(textItem.Text))
                {
                    yield return new ChatResponseUpdate(ChatRole.Assistant, textItem.Text);
                }
                else if (item is MessageItem messageItem)
                {
                    if (messageItem.IsSimpleText() && !string.IsNullOrEmpty(messageItem.GetSimpleText()))
                    {
                        yield return new ChatResponseUpdate(ChatRole.Assistant, messageItem.GetSimpleText());
                    }
                    else
                    {
                        foreach (var part in messageItem.Parts)
                        {
                            if (part is TextItem partText && !string.IsNullOrEmpty(partText.Text))
                            {
                                yield return new ChatResponseUpdate(ChatRole.Assistant, partText.Text);
                            }
                        }
                    }
                }
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

    private ChatSession GetReadyChatSessionOrThrow()
    {
        var fault = _state.Fault;

        if (fault != null)
        {
            throw new InvalidOperationException(
                "The Foundry Local model failed to prepare. See the application logs for details.",
                fault);
        }

        return _state.TryCreateChatSession()
            ?? throw new InvalidOperationException(
                "The Foundry Local model is still preparing. Please try again shortly.");
    }

    private static Request CreateRequest(IEnumerable<ChatMessage> messages, ChatOptions? options)
    {
        var request = new Request();

        if (options != null)
        {
            var search = new SearchOptions
            {
                Temperature = options.Temperature,
                MaxOutputTokens = options.MaxOutputTokens,
                TopP = options.TopP,
                TopK = options.TopK
            };

            if (search.Temperature.HasValue ||
                search.MaxOutputTokens.HasValue ||
                search.TopP.HasValue ||
                search.TopK.HasValue)
            {
                request.SetOptions(new RequestOptions { Search = search });
            }
        }

        foreach (var message in messages)
        {
            request.AddItem(ToMessageItem(message));
        }

        return request;
    }

    private static MessageItem ToMessageItem(ChatMessage message)
    {
        var role = message.Role == ChatRole.System
            ? MessageRole.System
            : message.Role == ChatRole.Assistant
                ? MessageRole.Assistant
                : message.Role == ChatRole.User
                    ? MessageRole.User
                    : MessageRole.User;

        var text = string.IsNullOrEmpty(message.Text) ? " " : message.Text;
        return new MessageItem(role, text);
    }

    private static string ExtractResponseText(Response response)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < response.ItemCount; i++)
        {
            var item = response.GetItem(i);
            if (item is MessageItem messageItem)
            {
                if (messageItem.IsSimpleText())
                {
                    sb.Append(messageItem.GetSimpleText());
                }
                else
                {
                    foreach (var part in messageItem.Parts)
                    {
                        if (part is TextItem textItem)
                        {
                            sb.Append(textItem.Text);
                        }
                    }
                }
            }
            else if (item is TextItem textItem)
            {
                sb.Append(textItem.Text);
            }
        }

        return sb.ToString();
    }
}
