namespace ProposalIQ.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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
    }
}