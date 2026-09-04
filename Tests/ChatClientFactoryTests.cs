using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Tests
{
    public class ChatClientFactoryTests
    {
        [Fact]
        public void Create_ReturnsChatClient_ForOpenRouterProvider()
        {
            var options = new AiProviderOptions
            {
                Provider = AiProvider.OpenRouter,
                OpenRouter = new OpenRouterOptions { Model = "openrouter/auto", Endpoint = "https://openrouter.ai/api/v1/", ApiKey = "key" }
            };

            var chatClient = ChatClientFactory.Create(options);

            Assert.NotNull(chatClient);
        }

        [Fact]
        public void Create_ReturnsChatClient_ForOpenAiProvider()
        {
            var options = new AiProviderOptions
            {
                Provider = AiProvider.OpenAI,
                OpenAI = new OpenAiOptions { Model = "gpt-4o-mini", ApiKey = "key" }
            };

            var chatClient = ChatClientFactory.Create(options);

            Assert.NotNull(chatClient);
        }

        [Fact]
        public void Create_ReturnsChatClient_ForAzureOpenAiProvider()
        {
            var options = new AiProviderOptions
            {
                Provider = AiProvider.AzureOpenAI,
                AzureOpenAI = new AzureOpenAiOptions { Endpoint = "https://example.openai.azure.com/", Deployment = "gpt-4o", ApiKey = "key" }
            };

            var chatClient = ChatClientFactory.Create(options);

            Assert.NotNull(chatClient);
        }

        [Fact]
        public void Create_ReturnsChatClient_ForLocalProvider_WithoutApiKey()
        {
            var options = new AiProviderOptions
            {
                Provider = AiProvider.Local,
                Local = new LocalOptions { Model = "llama3", Endpoint = "http://localhost:11434/v1/" }
            };

            var chatClient = ChatClientFactory.Create(options);

            Assert.NotNull(chatClient);
        }

        [Fact]
        public void Create_Throws_WhenRequiredSettingIsMissing()
        {
            var options = new AiProviderOptions
            {
                Provider = AiProvider.OpenAI,
                OpenAI = new OpenAiOptions { Model = "gpt-4o-mini", ApiKey = string.Empty }
            };

            var exception = Assert.Throws<InvalidOperationException>(() => ChatClientFactory.Create(options));
            Assert.Contains("Ai:OpenAI:ApiKey", exception.Message);
        }

        [Fact]
        public void Create_Throws_ForUnknownProvider()
        {
            var options = new AiProviderOptions { Provider = (AiProvider)999 };

            var exception = Assert.Throws<InvalidOperationException>(() => ChatClientFactory.Create(options));
            Assert.Contains("Unsupported AI provider", exception.Message);
        }
    }
}
