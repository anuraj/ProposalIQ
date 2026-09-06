## Context

See [proposal.md](proposal.md) for background and motivation.

Currently, ProposalIQ integrates with `Microsoft.AI.Foundry.Local` via `OpenAIChatClient` and `Model.GetChatClientAsync()`, which are marked `[Obsolete]` in Foundry Local SDK 2.x and scheduled for removal. In addition, `FoundryLocalModelPreparationService` initiates model downloading and loading without passing the `stoppingToken` or logging progress callbacks, leading to potential premature assumptions about model availability and silent failure during long downloads.

## Goals / Non-Goals

**Goals:**
- Replace deprecated `OpenAIChatClient` and `GetChatClientAsync` with `ChatSession` and native Foundry Local request models.
- Ensure `FoundryLocalModelPreparationService` awaits `model.DownloadAsync` with progress logging and cancellation support before calling `model.LoadAsync`.
- Eliminate all compiler obsolete warnings related to Foundry Local chat integration.
- Maintain seamless `IChatClient` abstraction compliance so `ProposalAnalysisService` works identically across all AI providers.

**Non-Goals:**
- Modifying other AI provider implementations (OpenAI, Azure OpenAI, OpenRouter, Local OpenAI-compatible).
- Implementing multi-turn conversation caching or custom tool definitions for Foundry Local.

## Decisions

### Decision 1: Use `ChatSession` and `Request`/`MessageItem` in `FoundryLocalChatClient`
- **Choice**: Implement `IChatClient.GetResponseAsync` and `IChatClient.GetStreamingResponseAsync` by instantiating or executing `ChatSession` on the loaded `IModel`.
- **Rationale**: `ChatSession` is the designated, non-obsolete API in `Microsoft.AI.Foundry.Local` 2.x. Mapping `Microsoft.Extensions.AI.ChatMessage` to `Microsoft.AI.Foundry.Local.MessageItem` (`Role` and `Text`) provides full compatibility with the rest of the application.
- **Alternatives Considered**:
  - Continuing to suppress `[Obsolete]` warnings: Rejected as `OpenAIChatClient` will be removed in subsequent SDK releases.
  - Calling the local web server endpoint: Rejected because in-process direct model invocation is lighter and does not require managing a separate HTTP daemon.

### Decision 2: Store loaded model or session factory in `FoundryLocalModelState`
- **Choice**: Update `FoundryLocalModelState` to store the initialized `IModel` (or `Func<ChatSession>`) instead of `OpenAIChatClient`.
- **Rationale**: Decouples session lifetime per request from the model lifecycle, preventing shared state collisions and eliminating obsolete types in the state container.

### Decision 3: Explicit download progress and cancellation in background preparation
- **Choice**: In `FoundryLocalModelPreparationService.ExecuteAsync`, pass `stoppingToken` to `GetModelAsync`, `DownloadAsync`, and `LoadAsync`, passing a progress callback action to `DownloadAsync` that logs download percentages at meaningful intervals.
- **Rationale**: Downloading local LLMs can take significant time depending on network speed and model size. Awaiting download completion with cancellation and progress reporting guarantees the model is fully cached before `LoadAsync` is invoked and allows graceful shutdown.

## Risks / Trade-offs

- **[Risk]** First request latency if a user attempts analysis while a multi-gigabyte model is still downloading.
  → **Mitigation**: `FoundryLocalModelState` continues to report `IsReady = false` until loading finishes, and `FoundryLocalChatClient` throws an actionable `InvalidOperationException` informing the user that the model is still preparing.
- **[Risk]** Thread safety when creating `ChatSession` instances.
  → **Mitigation**: Create `ChatSession(model)` per chat completion request or manage session synchronization safely.

## Migration Plan

1. Update `FoundryLocalModelState` to accept `IModel` or `Func<ChatSession>` upon `MarkReady`.
2. Refactor `FoundryLocalChatClient` to convert `Microsoft.Extensions.AI.ChatMessage` to `MessageItem`, execute via `ChatSession`, and extract completion text from `Response` / `StreamingResponse`.
3. Update `FoundryLocalModelPreparationService` to await `DownloadAsync` with progress logging and `stoppingToken` before calling `LoadAsync`.
4. Update unit tests in `Tests/` and run `dotnet test` and `dotnet build` to verify zero warnings and clean test passes.
