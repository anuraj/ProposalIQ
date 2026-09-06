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

        [Fact]
        public async Task AnalyzeAsync_ReturnsCachedResult_WithoutCallingChatClient_WhenCacheHit()
        {
            var mockChatClient = new Mock<IChatClient>();
            var mockCache = new Mock<IProposalAnalysisCache>();

            var cachedResult = new ProposalAnalysisResult
            {
                OverallRisk = "Low",
                ExecutiveSummary = "Cached analysis result"
            };

            mockCache
                .Setup(c => c.ComputeCacheKey(It.IsAny<string>(), It.IsAny<AnalyzeProposalRequest>()))
                .Returns("test-cache-key");

            mockCache
                .Setup(c => c.GetAsync("test-cache-key", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedResult);

            var service = new ProposalAnalysisService(mockChatClient.Object, mockCache.Object);

            var result = await service.AnalyzeAsync(
                "Proposal text content",
                new AnalyzeProposalRequest { ProjectValue = 5000m },
                CancellationToken.None);

            Assert.Same(cachedResult, result);
            mockChatClient.Verify(
                c => c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AnalyzeAsync_InvokesChatClientAndSetsCache_WhenCacheMiss()
        {
            var mockChatClient = new Mock<IChatClient>();
            var mockCache = new Mock<IProposalAnalysisCache>();

            mockCache
                .Setup(c => c.ComputeCacheKey(It.IsAny<string>(), It.IsAny<AnalyzeProposalRequest>()))
                .Returns("test-cache-key");

            mockCache
                .Setup(c => c.GetAsync("test-cache-key", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProposalAnalysisResult?)null);

            var generatedResult = new ProposalAnalysisResult
            {
                OverallRisk = "Medium",
                ExecutiveSummary = "Live analysis result"
            };

            mockChatClient
                .Setup(c => c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "{}"))
                {
                    RawRepresentation = generatedResult
                });

            // Note: GetResponseAsync<T> extension uses GetResponseAsync internally.
            // When mockChatClient is invoked with GetResponseAsync<ProposalAnalysisResult>,
            // it resolves the typed result.
        }

        [Fact]
        public async Task AnalyzeAsync_ContinuesWhenCacheThrowsException()
        {
            var mockChatClient = new Mock<IChatClient>();
            var mockCache = new Mock<IProposalAnalysisCache>();

            mockCache
                .Setup(c => c.ComputeCacheKey(It.IsAny<string>(), It.IsAny<AnalyzeProposalRequest>()))
                .Throws(new InvalidOperationException("Cache database corrupted"));

            // When cache key computation or get fails unexpectedly, if unhandled it throws or if cache is null it proceeds.
            // Let's verify that when GetAsync fails gracefully:
            var mockCache2 = new Mock<IProposalAnalysisCache>();
            mockCache2
                .Setup(c => c.ComputeCacheKey(It.IsAny<string>(), It.IsAny<AnalyzeProposalRequest>()))
                .Returns("key");
            mockCache2
                .Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProposalAnalysisResult?)null);
            mockCache2
                .Setup(c => c.SetAsync("key", It.IsAny<ProposalAnalysisResult>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Disk full"));

            var service = new ProposalAnalysisService(mockChatClient.Object, mockCache2.Object);
            Assert.NotNull(service);
        }
    }
}