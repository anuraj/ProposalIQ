using ProposalIQ.Web.Services;
using Moq;
using Microsoft.Extensions.AI;
using ProposalIQ.Web.Models;

namespace ProposalIQ.Web.Tests
{
    public class ProposalAnalysisServiceTests
    {
        [Fact]
        public async Task AnalyzeAsync_ThrowsArgumentException_WhenProposalTextIsEmpty()
        {
            var mockChatClient = new Mock<IChatClient>();
            var service = new ProposalAnalysisService(mockChatClient.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.AnalyzeAsync(
                    string.Empty,
                    new AnalyzeProposalRequest(),
                    CancellationToken.None));
        }
    }
}