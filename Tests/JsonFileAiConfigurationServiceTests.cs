using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Tests;

public class JsonFileAiConfigurationServiceTests
{
    [Fact]
    public async Task UpdateOptionsAsync_UpdatesInMemoryAndFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        var initial = new AiProviderOptions { Provider = AiProvider.FoundryLocal };

        try
        {
            var service = new JsonFileAiConfigurationService(
                Options.Create(initial),
                NullLogger<JsonFileAiConfigurationService>.Instance,
                tempFile);

            var updated = new AiProviderOptions
            {
                Provider = AiProvider.OpenAI,
                OpenAI = new OpenAiOptions { Model = "gpt-4o", ApiKey = "sk-test" }
            };

            await service.UpdateOptionsAsync(updated);

            var loaded = service.GetOptions();
            Assert.Equal(AiProvider.OpenAI, loaded.Provider);
            Assert.Equal("gpt-4o", loaded.OpenAI.Model);
            Assert.Equal("sk-test", loaded.OpenAI.ApiKey);

            // Verify file was written
            Assert.True(File.Exists(tempFile));
            var service2 = new JsonFileAiConfigurationService(
                Options.Create(initial),
                NullLogger<JsonFileAiConfigurationService>.Instance,
                tempFile);

            Assert.Equal(AiProvider.OpenAI, service2.GetOptions().Provider);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
