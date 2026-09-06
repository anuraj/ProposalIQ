using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Tests;

public class FoundryLocalModelStateTests
{
    [Fact]
    public void IsReady_IsFalse_BeforePreparationCompletes()
    {
        var state = new FoundryLocalModelState();

        Assert.False(state.IsReady);
        Assert.Null(state.TryGetChatClient());
    }

    [Fact]
    public void MarkFaulted_RecordsFault_WithoutBecomingReady()
    {
        var state = new FoundryLocalModelState();
        var error = new InvalidOperationException("model download failed");

        state.MarkFaulted(error);

        Assert.False(state.IsReady);
        Assert.Same(error, state.Fault);
    }
}
