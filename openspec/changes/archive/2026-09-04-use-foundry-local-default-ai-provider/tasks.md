## 1. Provider And Options Tests

- [x] 1.1 Add or update tests showing new `AiProviderOptions` instances default to `FoundryLocal` and verify the test fails before implementation.
- [x] 1.2 Add or update tests showing default `FoundryLocalOptions` include a Qwen-family model alias and verify the test fails before implementation.
- [x] 1.3 Add or update `ChatClientFactory` tests showing explicit `OpenRouter`, `OpenAI`, `AzureOpenAI`, and `Local` provider configuration remains selectable and unaffected.
- [x] 1.4 Add a `ChatClientFactory` test for `FoundryLocal` that throws `InvalidOperationException` naming `Ai:FoundryLocal:ModelAlias` when the model alias is blank.

## 2. Foundry Local Provider Wiring

- [x] 2.1 Add the `Microsoft.AI.Foundry.Local` package reference to `Src/ProposalIQ.Web.csproj` and verify the project restores and builds.
- [x] 2.2 Add `AiProvider.FoundryLocal` and a `FoundryLocalOptions` class (configurable `ModelAlias`, default a Qwen-family alias; `AppName`) to `AiProviderOptions`, and change the default `Provider` to `FoundryLocal`, and verify the options-default tests pass.
- [x] 2.3 Add a thread-safe model readiness state type that tracks "preparing", "ready" (holding the SDK chat client), or "faulted" (holding the error), and verify it is unit-testable without the real Foundry Local runtime.
- [x] 2.4 Add a background hosted service that initializes `FoundryLocalManager`, resolves the configured model alias from the catalog, downloads and loads it, and publishes the result into the readiness state, and verify it is only registered when `FoundryLocal` is the selected provider.
- [x] 2.5 Add an `IChatClient` adapter that checks the readiness state before each call, throwing a clear "model not ready" or "model preparation failed" error when not ready, and otherwise forwarding chat requests to the underlying SDK chat client, and verify the not-ready and faulted paths are covered by tests.
- [x] 2.6 Wire `ChatClientFactory` and `Program.cs` so `FoundryLocal` returns the adapter as the registered `IChatClient` and the background hosted service is registered, without changing how other providers are constructed, and verify `dotnet build` succeeds.

## 3. Configuration And Documentation

- [x] 3.1 Update sample application configuration (`appsettings.json`) to make `FoundryLocal` the default provider with its model alias setting, and verify no API keys are added to source control.
- [x] 3.2 Update README or setup documentation with Foundry Local usage (prerequisite runtime, default model alias, override instructions) and OpenRouter override guidance, and verify the documented settings match the implemented option names.

## 4. Validation

- [x] 4.1 Run focused AI provider and Foundry Local readiness tests and verify they pass.
- [x] 4.2 Run `dotnet test` from `Tests/` and verify the full test suite passes.
- [x] 4.3 Run `openspec validate use-foundry-local-default-ai-provider --strict` and verify the change passes OpenSpec validation.
