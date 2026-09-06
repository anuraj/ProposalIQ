## Context

See proposal.md for motivation. The application already has an `AiProvider.Local` path that creates a generic OpenAI-compatible HTTP client without requiring an API key (used for runtimes like Ollama). Foundry Local instead ships an official `Microsoft.AI.Foundry.Local` .NET SDK (`FoundryLocalManager`) that manages model discovery, download, loading, and chat completions itself; it does not expose a plain HTTP endpoint the existing `Local` provider path can call. This supersedes the original design decision to reuse the HTTP-based `Local` path for Foundry Local.

## Goals / Non-Goals

**Goals:**
- Add a distinct `FoundryLocal` provider that becomes the default when no provider is explicitly configured.
- Use the `Microsoft.AI.Foundry.Local` SDK to initialize the runtime, resolve the configured model, download it if missing, and load it.
- Run model download/load in the background at startup so the host is not blocked; reject proposal analysis with a clear "model not ready" error until preparation finishes or report a clear error if preparation failed.
- Keep the model alias configurable, defaulting to a Qwen-family model id.
- Keep OpenRouter, OpenAI, Azure OpenAI, and the existing generic `Local` provider behavior unchanged and explicitly selectable.
- Bridge the SDK's chat client into the application's existing `Microsoft.Extensions.AI.IChatClient` abstraction so the rest of the app (`ProposalAnalysisService`, DI registration) is unaffected.

**Non-Goals:**
- Install or manage the Foundry Local host runtime itself; the SDK assumes Foundry Local Core is available on the machine.
- Health-check or block application startup on model readiness.
- Configure hardware acceleration (WinML execution providers), audio transcription, or the SDK's optional web service.
- Remove OpenRouter or generic local (`Local`) provider support, or migrate existing user secrets.
- Provide integration tests that require a real Foundry Local runtime or model download; those are out of reach in this environment and remain a manual verification step.

## Decisions

### Add a distinct `FoundryLocal` provider instead of reusing `Local`

Introduce `AiProvider.FoundryLocal` as a new enum value and make it the default. The existing `Local` provider keeps its current generic OpenAI-compatible HTTP behavior for explicit configuration (e.g., Ollama-style runtimes).

Rationale: Foundry Local is SDK-driven, not a plain HTTP endpoint, so its request/response handling is fundamentally different from the generic `Local` path. A distinct provider keeps that path free of SDK-specific lifecycle concerns (model download/load, readiness) while making Foundry Local's behavior explicit and testable.

Alternatives considered:
- Reuse `Local` for Foundry Local (the original decision). Rejected because Foundry Local has no configurable HTTP endpoint to point the generic client at; it requires the SDK's model lifecycle APIs.

### Prepare the model in a background hosted service, gate requests on readiness

At startup, when `FoundryLocal` is the selected provider, register a background hosted service that calls `FoundryLocalManager.CreateAsync`, resolves the catalog, looks up the configured model alias, downloads it if not cached, and loads it. A small thread-safe state object records whether preparation succeeded, is still in progress, or failed. The `IChatClient` adapter consults this state on every call and throws a clear, descriptive error (surfaced through the existing controller error handling) when the model is not yet ready or preparation failed.

Rationale: model download/load can take a long time on first run and must not block application startup or other providers. Gating requests instead of blocking startup keeps existing behavior for OpenRouter/OpenAI/AzureOpenAI/Local unaffected and gives users an explicit, actionable error instead of a hang or crash.

Alternatives considered:
- Block startup until the model is ready. Rejected because first-run downloads can be large and slow, and would delay the entire host including unrelated providers.
- Health-check the model before accepting requests via a readiness probe/middleware. Rejected as unnecessary complexity for this change; the existing per-request error handling in `HomeController.Analyze` already reports exceptions to the user.

### Bridge the SDK chat client through an `IChatClient` adapter

The SDK's `model.GetChatClientAsync()` returns the SDK's own `OpenAIChatClient` (built on `Betalgo.Ranul.OpenAI` types), not `Microsoft.Extensions.AI.IChatClient`. Add an adapter class implementing `IChatClient` that forwards to the SDK chat client once the background state reports readiness, translating messages and responses between the two type systems.

Rationale: the rest of the application (`ProposalAnalysisService`, DI registration via `AddChatClient`) is written against `Microsoft.Extensions.AI.IChatClient` and should not need to change. An adapter isolates the SDK-specific types to one place.

Alternatives considered:
- Change `ProposalAnalysisService` and DI to use the SDK's chat client type directly. Rejected because it would leak a provider-specific type through the whole application and break support for the other providers, which already return `IChatClient`.

### Keep the model alias configurable with a Qwen-family default

Add a `FoundryLocal` options section with a configurable `ModelAlias` (defaulting to a Qwen-family alias) and an `AppName` used to initialize the SDK. Fail-fast validation requires a nonblank model alias; no API key or endpoint is required.

Rationale: matches the existing per-provider options pattern and keeps the default model overridable without code changes.

Alternatives considered:
- Hardcode the model alias. Rejected because users need to override the local model through configuration, consistent with other providers.

## Risks / Trade-offs

- [Risk] Foundry Local Core or the configured model may not be available on the host machine -> Mitigation: background preparation reports failure through the shared state; the chat client adapter surfaces this as a clear per-request error instead of crashing startup.
- [Risk] Users relying on implicit OpenRouter behavior will see a default-provider change -> Mitigation: keep explicit `Ai:Provider=OpenRouter` support and document the override.
- [Risk] First-run model download can be slow, leaving requests rejected as "not ready" for an extended period -> Mitigation: document this in sample configuration/README; this is an accepted trade-off of background preparation.
- [Risk] The adapter's message/response translation between `Microsoft.Extensions.AI` and `Betalgo.Ranul.OpenAI` types may not cover every field the SDK supports -> Mitigation: cover basic single-turn request/response text content, matching what `ProposalAnalysisService` requires today; treat richer translation as future work if needed.
- [Risk] This environment cannot install or run the actual Foundry Local runtime, so the download/load/inference path cannot be integration-tested here -> Mitigation: unit test the readiness-gating and option/validation behavior; document manual verification against a real Foundry Local install as a follow-up.

## Migration Plan

No data migration is required. Existing deployments that want OpenRouter as the default behavior should set `Ai:Provider` to `OpenRouter` and keep their OpenRouter API key, endpoint, and model configuration. Rollback restores `OpenRouter` as the options default and removes the `FoundryLocal` provider selection.