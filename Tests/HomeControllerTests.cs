namespace ProposalIQ.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using ProposalIQ.Web.Controllers;
    using Xunit;
    using ProposalIQ.Web.Services;
    using Moq;
    using ProposalIQ.Web.Models;

    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsAViewResult()
        {
            var mockProposalTextExtractor = new Mock<IProposalTextExtractor>();
            var mockProposalAnalysisService = new Mock<IProposalAnalysisService>();
            // Arrange
            var controller = new HomeController(mockProposalTextExtractor.Object, mockProposalAnalysisService.Object);

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ReturnsAViewResult()
        {
            var mockProposalTextExtractor = new Mock<IProposalTextExtractor>();
            var mockProposalAnalysisService = new Mock<IProposalAnalysisService>();
            // Arrange
            var controller = new HomeController(mockProposalTextExtractor.Object, mockProposalAnalysisService.Object);

            // Act
            var result = controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Analyze_ReturnsViewResult_WhenProposalFileIsNull()
        {
            var mockProposalTextExtractor = new Mock<IProposalTextExtractor>();
            var mockProposalAnalysisService = new Mock<IProposalAnalysisService>();

            // Arrange
            var controller = new HomeController(mockProposalTextExtractor.Object, mockProposalAnalysisService.Object);

            // Act
            var result = await controller.Analyze(new AnalyzeProposalRequest { ProposalFile = null }, CancellationToken.None);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState["ProposalFile"]!.Errors, e => e.ErrorMessage.Contains("upload a proposal"));
            Assert.NotNull(controller.ViewBag.ModelStatus);
        }

        [Fact]
        public async Task Analyze_ReturnsIndexViewAndDoesNotAnalyze_WhenExtractedTextIsEmpty()
        {
            var mockProposalTextExtractor = new Mock<IProposalTextExtractor>();
            var mockProposalAnalysisService = new Mock<IProposalAnalysisService>();
            var file = new FormFile(
                new MemoryStream(Array.Empty<byte>()),
                0,
                1,
                "proposalFile",
                "proposal.pptx");

            mockProposalTextExtractor
                .Setup(extractor => extractor.ExtractTextAsync(file, It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            var controller = new HomeController(mockProposalTextExtractor.Object, mockProposalAnalysisService.Object);

            var result = await controller.Analyze(
                new AnalyzeProposalRequest { ProposalFile = file },
                CancellationToken.None);

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState["ProposalFile"]!.Errors, e => e.ErrorMessage.Contains("No readable text"));
            Assert.NotNull(controller.ViewBag.ModelStatus);
            mockProposalAnalysisService.Verify(
                service => service.AnalyzeAsync(
                    It.IsAny<string>(),
                    It.IsAny<AnalyzeProposalRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Analyze_ReturnsIndexView_WithModelError_WhenAnalysisFailsWithException()
        {
            var mockProposalTextExtractor = new Mock<IProposalTextExtractor>();
            var mockProposalAnalysisService = new Mock<IProposalAnalysisService>();
            var file = new FormFile(
                new MemoryStream(Encoding.UTF8.GetBytes("Sample proposal text")),
                0,
                20,
                "proposalFile",
                "proposal.txt");

            mockProposalTextExtractor
                .Setup(extractor => extractor.ExtractTextAsync(file, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Sample proposal text");

            mockProposalAnalysisService
                .Setup(service => service.AnalyzeAsync(It.IsAny<string>(), It.IsAny<AnalyzeProposalRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("AI model rate limit exceeded"));

            var controller = new HomeController(mockProposalTextExtractor.Object, mockProposalAnalysisService.Object);

            var result = await controller.Analyze(
                new AnalyzeProposalRequest { ProposalFile = file, ProjectValue = 5000m },
                CancellationToken.None);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState["ProposalFile"]!.Errors, e => e.ErrorMessage.Contains("AI model rate limit exceeded"));
            Assert.NotNull(controller.ViewBag.ModelStatus);
        }

        [Fact]
        public async Task Analyze_ReturnsIndexView_WithModelError_WhenCancelled()
        {
            var mockProposalTextExtractor = new Mock<IProposalTextExtractor>();
            var mockProposalAnalysisService = new Mock<IProposalAnalysisService>();
            var file = new FormFile(
                new MemoryStream(Encoding.UTF8.GetBytes("Sample proposal text")),
                0,
                20,
                "proposalFile",
                "proposal.txt");

            mockProposalTextExtractor
                .Setup(extractor => extractor.ExtractTextAsync(file, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var controller = new HomeController(mockProposalTextExtractor.Object, mockProposalAnalysisService.Object);

            var result = await controller.Analyze(
                new AnalyzeProposalRequest { ProposalFile = file },
                CancellationToken.None);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState["ProposalFile"]!.Errors, e => e.ErrorMessage.Contains("Analysis was cancelled"));
            Assert.NotNull(controller.ViewBag.ModelStatus);
        }

        [Fact]
        public void ModelStatus_ReturnsReady_ForNonFoundryLocalProvider()
        {
            var mockProposalTextExtractor = new Mock<IProposalTextExtractor>();
            var mockProposalAnalysisService = new Mock<IProposalAnalysisService>();
            var mockConfigService = new Mock<IAiConfigurationService>();
            mockConfigService.Setup(c => c.GetOptions()).Returns(new Configuration.AiProviderOptions { Provider = Configuration.AiProvider.OpenAI });

            var controller = new HomeController(
                mockProposalTextExtractor.Object,
                mockProposalAnalysisService.Object,
                mockConfigService.Object,
                new FoundryLocalModelState());

            var result = controller.ModelStatus();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var status = Assert.IsType<AiModelStatusViewModel>(jsonResult.Value);

            Assert.True(status.IsReady);
            Assert.False(status.IsFaulted);
            Assert.Equal("OpenAI", status.Provider);
            Assert.Equal(100, status.ProgressPercent);
        }

        [Fact]
        public void ModelStatus_ReturnsDownloadingState_ForFoundryLocalDuringDownload()
        {
            var mockProposalTextExtractor = new Mock<IProposalTextExtractor>();
            var mockProposalAnalysisService = new Mock<IProposalAnalysisService>();
            var mockConfigService = new Mock<IAiConfigurationService>();
            mockConfigService.Setup(c => c.GetOptions()).Returns(new Configuration.AiProviderOptions { Provider = Configuration.AiProvider.FoundryLocal });
            var foundryLocalState = new FoundryLocalModelState();
            foundryLocalState.SetDownloading(55, "Downloading model weights...");

            var controller = new HomeController(
                mockProposalTextExtractor.Object,
                mockProposalAnalysisService.Object,
                mockConfigService.Object,
                foundryLocalState);

            var result = controller.ModelStatus();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var status = Assert.IsType<AiModelStatusViewModel>(jsonResult.Value);

            Assert.False(status.IsReady);
            Assert.False(status.IsFaulted);
            Assert.Equal("FoundryLocal", status.Provider);
            Assert.Equal(55, status.ProgressPercent);
            Assert.Equal("Downloading", status.Stage);
            Assert.Equal("Downloading model weights...", status.Message);
        }

        [Fact]
        public void ModelStatus_ReturnsFaultedState_WhenFoundryLocalPreparationFails()
        {
            var mockProposalTextExtractor = new Mock<IProposalTextExtractor>();
            var mockProposalAnalysisService = new Mock<IProposalAnalysisService>();
            var mockConfigService = new Mock<IAiConfigurationService>();
            mockConfigService.Setup(c => c.GetOptions()).Returns(new Configuration.AiProviderOptions { Provider = Configuration.AiProvider.FoundryLocal });
            var foundryLocalState = new FoundryLocalModelState();
            foundryLocalState.MarkFaulted(new InvalidOperationException("Download server unavailable"), "Failed to download model");

            var controller = new HomeController(
                mockProposalTextExtractor.Object,
                mockProposalAnalysisService.Object,
                mockConfigService.Object,
                foundryLocalState);

            var result = controller.ModelStatus();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var status = Assert.IsType<AiModelStatusViewModel>(jsonResult.Value);

            Assert.False(status.IsReady);
            Assert.True(status.IsFaulted);
            Assert.Equal("FoundryLocal", status.Provider);
            Assert.Equal("Faulted", status.Stage);
            Assert.Equal("Failed to download model", status.Message);
        }
    }
}