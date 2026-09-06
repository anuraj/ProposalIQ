using Microsoft.AI.Foundry.Local;

namespace ProposalIQ.Web.Services;

// Shared between the background preparation service and the chat client adapter.
public class FoundryLocalModelState
{
    private readonly object _gate = new();
    private OpenAIChatClient? _chatClient;
    private Exception? _fault;

    public bool IsReady
    {
        get { lock (_gate) { return _chatClient != null; } }
    }

    public Exception? Fault
    {
        get { lock (_gate) { return _fault; } }
    }

    public void MarkReady(OpenAIChatClient chatClient)
    {
        lock (_gate)
        {
            _chatClient = chatClient;
            _fault = null;
        }
    }

    public void MarkFaulted(Exception exception)
    {
        lock (_gate)
        {
            _fault = exception;
        }
    }

    public OpenAIChatClient? TryGetChatClient()
    {
        lock (_gate) { return _chatClient; }
    }
}
