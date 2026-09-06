using Microsoft.Extensions.AI;
using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Tests;

public class FoundryLocalChatClientTests
{
    [Fact]
    public async Task GetResponseAsync_Throws_WhenModelIsStillPreparing()
    {
        var state = new FoundryLocalModelState();
        var client = new FoundryLocalChatClient(state);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetResponseAsync([new ChatMessage(ChatRole.User, "Hello")]));

        Assert.Contains("still preparing", exception.Message);
    }

    [Fact]
    public async Task GetResponseAsync_Throws_WhenModelPreparationFailed()
    {
        var state = new FoundryLocalModelState();
        state.MarkFaulted(new InvalidOperationException("download failed"));
        var client = new FoundryLocalChatClient(state);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetResponseAsync([new ChatMessage(ChatRole.User, "Hello")]));

        Assert.Contains("failed to prepare", exception.Message);
    }
}
