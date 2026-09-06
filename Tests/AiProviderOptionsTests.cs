using ProposalIQ.Web.Configuration;

namespace ProposalIQ.Web.Tests;

public class AiProviderOptionsTests
{
    [Fact]
    public void Provider_DefaultsToFoundryLocal()
    {
        var options = new AiProviderOptions();

        Assert.Equal(AiProvider.FoundryLocal, options.Provider);
    }

    [Fact]
    public void FoundryLocalOptions_DefaultsToQwenFamilyModelAlias()
    {
        var options = new AiProviderOptions();

        Assert.StartsWith("qwen", options.FoundryLocal.ModelAlias, StringComparison.OrdinalIgnoreCase);
    }
}
