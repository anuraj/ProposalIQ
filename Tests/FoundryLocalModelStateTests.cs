using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Tests;

public class FoundryLocalModelStateTests
{
    [Fact]
    public void IsReady_IsFalse_BeforePreparationCompletes()
    {
        var state = new FoundryLocalModelState();

        Assert.False(state.IsReady);
        Assert.Null(state.TryCreateChatSession());
    }

    [Fact]
    public void MarkFaulted_RecordsFault_WithoutBecomingReady()
    {
        var state = new FoundryLocalModelState();
        var error = new InvalidOperationException("model download failed");

        state.MarkFaulted(error);

        Assert.False(state.IsReady);
        Assert.Same(error, state.Fault);
        Assert.Null(state.TryCreateChatSession());
    }

    [Fact]
    public void MarkReady_WithSessionFactory_SetsIsReadyTrue_AndClearsFault()
    {
        var state = new FoundryLocalModelState();
        state.MarkFaulted(new InvalidOperationException("initial failure"));

        state.MarkReady(() => null!);

        Assert.True(state.IsReady);
        Assert.Null(state.Fault);
    }

    [Fact]
    public void MarkFaulted_AfterMarkReady_ResetsReady_AndSetsFault()
    {
        var state = new FoundryLocalModelState();
        state.MarkReady(() => null!);
        Assert.True(state.IsReady);

        var error = new InvalidOperationException("runtime fault");
        state.MarkFaulted(error);

        Assert.False(state.IsReady);
        Assert.Same(error, state.Fault);
        Assert.Null(state.TryCreateChatSession());
    }

    [Fact]
    public void SetDownloading_SetsStageAndProgress()
    {
        var state = new FoundryLocalModelState();
        state.SetDownloading(45, "Downloading model weights...");

        Assert.Equal(FoundryLocalPreparationStage.Downloading, state.Stage);
        Assert.Equal(45, state.ProgressPercent);
        Assert.Equal("Downloading model weights...", state.StatusMessage);
        Assert.False(state.IsReady);
    }

    [Fact]
    public void SetLoading_SetsStageAndProgress100()
    {
        var state = new FoundryLocalModelState();
        state.SetLoading("Loading model...");

        Assert.Equal(FoundryLocalPreparationStage.Loading, state.Stage);
        Assert.Equal(100, state.ProgressPercent);
        Assert.Equal("Loading model...", state.StatusMessage);
        Assert.False(state.IsReady);
    }

    [Fact]
    public void MarkReady_SetsStageReadyAndProgress100()
    {
        var state = new FoundryLocalModelState();
        state.MarkReady(() => null!);

        Assert.Equal(FoundryLocalPreparationStage.Ready, state.Stage);
        Assert.Equal(100, state.ProgressPercent);
        Assert.Equal("Model is ready.", state.StatusMessage);
        Assert.True(state.IsReady);
    }
}
