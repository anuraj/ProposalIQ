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
            Assert.IsType<ViewResult>(result);
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
            mockProposalAnalysisService.Verify(
                service => service.AnalyzeAsync(
                    It.IsAny<string>(),
                    It.IsAny<AnalyzeProposalRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}