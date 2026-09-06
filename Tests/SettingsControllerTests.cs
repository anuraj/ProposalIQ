using Microsoft.AspNetCore.Mvc;
using Moq;
using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Controllers;
using ProposalIQ.Web.Models;
using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Tests;

public class SettingsControllerTests
{
    [Fact]
    public void Index_Get_ReturnsViewWithConfiguredOptions()
    {
        var mockConfigService = new Mock<IAiConfigurationService>();
        var mockModelManager = new Mock<IFoundryLocalModelManager>();

        mockConfigService.Setup(c => c.GetOptions()).Returns(new AiProviderOptions
        {
            Provider = AiProvider.OpenAI,
            OpenAI = new OpenAiOptions { Model = "gpt-4o", ApiKey = "sk-secret" }
        });

        var controller = new SettingsController(mockConfigService.Object, mockModelManager.Object);

        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SettingsViewModel>(viewResult.Model);

        Assert.Equal(AiProvider.OpenAI, model.ActiveProvider);
        Assert.Equal("gpt-4o", model.OpenAI.Model);
        Assert.True(model.HasOpenAiApiKey);
        Assert.Equal(string.Empty, model.OpenAI.ApiKey); // Masked
    }

    [Fact]
    public async Task Index_Post_ValidOpenAi_SavesAndReturnsSuccess()
    {
        var mockConfigService = new Mock<IAiConfigurationService>();
        var mockModelManager = new Mock<IFoundryLocalModelManager>();

        var existingOptions = new AiProviderOptions
        {
            Provider = AiProvider.FoundryLocal,
            OpenAI = new OpenAiOptions { Model = "gpt-4o-mini", ApiKey = "existing-key" }
        };

        mockConfigService.Setup(c => c.GetOptions()).Returns(existingOptions);

        var controller = new SettingsController(mockConfigService.Object, mockModelManager.Object);

        var postModel = new SettingsViewModel
        {
            ActiveProvider = AiProvider.OpenAI,
            OpenAI = new OpenAiOptions
            {
                Model = "gpt-4o",
                ApiKey = string.Empty // Keep existing
            }
        };

        var result = await controller.Index(postModel, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SettingsViewModel>(viewResult.Model);

        Assert.NotNull(model.SuccessMessage);
        Assert.Equal(AiProvider.OpenAI, model.ActiveProvider);

        mockConfigService.Verify(c => c.UpdateOptionsAsync(
            It.Is<AiProviderOptions>(opt => opt.Provider == AiProvider.OpenAI && opt.OpenAI.ApiKey == "existing-key" && opt.OpenAI.Model == "gpt-4o"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Index_Post_MissingRequiredFields_ReturnsValidationError()
    {
        var mockConfigService = new Mock<IAiConfigurationService>();
        var mockModelManager = new Mock<IFoundryLocalModelManager>();

        mockConfigService.Setup(c => c.GetOptions()).Returns(new AiProviderOptions
        {
            Provider = AiProvider.OpenAI,
            OpenAI = new OpenAiOptions { Model = "", ApiKey = "" }
        });

        var controller = new SettingsController(mockConfigService.Object, mockModelManager.Object);

        var postModel = new SettingsViewModel
        {
            ActiveProvider = AiProvider.OpenAI,
            OpenAI = new OpenAiOptions { Model = "", ApiKey = "" }
        };

        var result = await controller.Index(postModel, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("OpenAI.Model"));
        Assert.True(controller.ModelState.ContainsKey("OpenAI.ApiKey"));
    }
}
