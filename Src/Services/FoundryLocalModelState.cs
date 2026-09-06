using Microsoft.AI.Foundry.Local;

namespace ProposalIQ.Web.Services;

public enum FoundryLocalPreparationStage
{
    NotStarted,
    Downloading,
    Loading,
    Ready,
    Faulted
}

// Shared between the background preparation service and the chat client adapter.
public class FoundryLocalModelState
{
    private readonly object _gate = new();
    private Func<ChatSession>? _sessionFactory;
    private Exception? _fault;
    private FoundryLocalPreparationStage _stage = FoundryLocalPreparationStage.NotStarted;
    private int _progressPercent = 0;
    private string _statusMessage = "Model preparation not started.";

    public bool IsReady
    {
        get
        {
            lock (_gate)
            {
                return _sessionFactory != null;
            }
        }
    }

    public Exception? Fault
    {
        get
        {
            lock (_gate)
            {
                return _fault;
            }
        }
    }

    public FoundryLocalPreparationStage Stage
    {
        get
        {
            lock (_gate)
            {
                return _stage;
            }
        }
    }

    public int ProgressPercent
    {
        get
        {
            lock (_gate)
            {
                return _progressPercent;
            }
        }
    }

    public string StatusMessage
    {
        get
        {
            lock (_gate)
            {
                return _statusMessage;
            }
        }
    }

    public void SetDownloading(int progressPercent, string message = "Downloading model...")
    {
        lock (_gate)
        {
            _stage = FoundryLocalPreparationStage.Downloading;
            _progressPercent = Math.Clamp(progressPercent, 0, 100);
            _statusMessage = message;
        }
    }

    public void SetLoading(string message = "Loading model into memory...")
    {
        lock (_gate)
        {
            _stage = FoundryLocalPreparationStage.Loading;
            _progressPercent = 100;
            _statusMessage = message;
        }
    }

    public void MarkReady(IModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        lock (_gate)
        {
            _sessionFactory = () => new ChatSession(model);
            _fault = null;
            _stage = FoundryLocalPreparationStage.Ready;
            _progressPercent = 100;
            _statusMessage = "Model is ready.";
        }
    }

    public void MarkReady(Func<ChatSession> sessionFactory)
    {
        ArgumentNullException.ThrowIfNull(sessionFactory);

        lock (_gate)
        {
            _sessionFactory = sessionFactory;
            _fault = null;
            _stage = FoundryLocalPreparationStage.Ready;
            _progressPercent = 100;
            _statusMessage = "Model is ready.";
        }
    }

    public void MarkFaulted(Exception exception, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        lock (_gate)
        {
            _sessionFactory = null;
            _fault = exception;
            _stage = FoundryLocalPreparationStage.Faulted;
            _statusMessage = message ?? $"Model preparation failed: {exception.Message}";
        }
    }

    public ChatSession? TryCreateChatSession()
    {
        lock (_gate)
        {
            return _sessionFactory?.Invoke();
        }
    }
}
