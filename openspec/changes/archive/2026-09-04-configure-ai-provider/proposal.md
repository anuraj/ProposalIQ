# Configure AI Provider

## Why

`Program.cs` hardcodes the chat client to OpenRouter (`OpenRouter:ApiKey`, fixed endpoint, `openrouter/auto` model). Users who already have an OpenAI account, an Azure OpenAI deployment, or a local OpenAI-compatible model server (e.g. Ollama, LM Studio) cannot use ProposalIQ without routing through OpenRouter. We need a configuration-driven way to select the AI provider at startup.

## What Changes

- Add an `Ai` configuration section with a `Provider` selector (`OpenRouter`, `OpenAI`, `AzureOpenAI`, `Local`) plus per-provider settings (model/deployment name, endpoint, API key).
- Introduce a small factory responsible for building the `IChatClient` registered with DI, choosing the concrete client construction based on the configured provider.
- Add the `Azure.AI.OpenAI` package reference needed to build an Azure OpenAI-backed chat client.
- Keep OpenRouter as the default provider so existing deployments and user-secrets configuration keep working without changes.
- Validate configuration at startup and fail fast with a clear error message when required settings for the selected provider are missing.
- Document the new configuration keys and the supported providers in `appsettings.json` (with placeholder/empty values) and `README.md`.

## Non-goals

- No UI/admin screen for switching providers at runtime; selection is via configuration only and requires an app restart.
- No support for provider-specific advanced features (e.g. Azure OpenAI content filters, OpenAI Assistants API) beyond basic chat completion.
- No credential storage mechanism beyond what ASP.NET Core configuration providers (user secrets, environment variables, Key Vault, etc.) already offer.
- No changes to `ProposalAnalysisService` or other consumers of `IChatClient` — they keep depending on `IChatClient` and are provider-agnostic.

## Impact

- Affected code: `Src/Program.cs`, `Src/appsettings.json`, `Src/appsettings.Development.json`, `Directory.Packages.props`, `Src/ProposalIQ.Web.csproj`.
- New code: an AI provider options type and a chat client factory (`Src/Services` or `Src/Configuration`).
- Tests: new unit tests for the factory's provider-selection logic; existing tests continue to pass unmodified.
