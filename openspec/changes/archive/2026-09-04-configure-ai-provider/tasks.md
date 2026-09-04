# Tasks: Configure AI Provider

## 1. Configuration model
- [x] 1.1 Add `Src/Configuration/AiProviderOptions.cs` with `AiProvider` enum and nested options (`OpenRouterOptions`, `OpenAiOptions`, `AzureOpenAiOptions`, `LocalOptions`) as described in design.md.
- [x] 1.2 Add the `Ai` section (with empty/placeholder secrets) to `Src/appsettings.json`, defaulting `Provider` to `OpenRouter`.

## 2. Package reference
- [x] 2.1 Add `Azure.AI.OpenAI` to `Directory.Packages.props` (version compatible with `Microsoft.Extensions.AI.OpenAI` 10.9.0) and reference it in `Src/ProposalIQ.Web.csproj`.

## 3. Chat client factory
- [x] 3.1 Add `Src/Services/ChatClientFactory.cs` implementing `Create(AiProviderOptions)` with per-provider construction and required-field validation, throwing `InvalidOperationException` with a clear message for missing settings or an unknown provider value.
- [x] 3.2 Add unit tests in `Tests/` covering: default OpenRouter path, OpenAI path, AzureOpenAI path, Local path, missing-required-setting failure, and unknown-provider failure.

## 4. Wire up Program.cs
- [x] 4.1 Update `Src/Program.cs` to bind `AiProviderOptions` from configuration, apply the legacy `OpenRouter:ApiKey` fallback, and call `ChatClientFactory.Create(...)` instead of the inline OpenRouter-only construction.
- [x] 4.2 Run `dotnet build` and `dotnet test` from `Tests/` to confirm the app still starts with existing OpenRouter user-secrets configuration and all tests pass.

## 5. Documentation
- [x] 5.1 Update `README.md` with the new `Ai` configuration keys, supported provider values, and an example for each provider (OpenRouter, OpenAI, AzureOpenAI, Local).
