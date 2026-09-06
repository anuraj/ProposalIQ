## Context

See [proposal.md](proposal.md) for background and motivation.

When the application runs with the `FoundryLocal` AI provider, `FoundryLocalModelPreparationService` downloads and loads the specified model asynchronously. However, `Index.cshtml` and `site.js` currently assume the chat client is always immediately ready, enabling the analyze button as soon as any valid file is selected. If the model is still downloading, the user triggers an analysis that fails with an unhandled preparation exception.

## Goals / Non-Goals

**Goals:**
- Provide real-time visibility into AI provider model preparation state (stage, progress, fault).
- Show a clear warning banner and progress indicator on the proposal analysis page while the model is downloading or loading in the background.
- Keep the "Analyze Proposal" button disabled while model preparation is incomplete, even if a proposal file has been selected.
- Automatically enable the button and clear the warning once preparation completes (and a valid file is selected).
- Gracefully handle other AI providers (OpenAI, OpenRouter, Azure OpenAI, Local) by reporting ready immediately.

**Non-Goals:**
- Allowing manual pausing or cancellation of model downloads from the UI.
- WebSockets or SignalR dependencies for status updates (lightweight periodic polling is sufficient and dependency-free).

## Decisions

### Decision 1: Model status tracking in `FoundryLocalModelState`
- **Choice**: Extend `FoundryLocalModelState` with status properties: `Stage` (`Preparing`, `Downloading`, `Loading`, `Ready`, `Faulted`), `ProgressPercent`, and optional status messages. Update these during `FoundryLocalModelPreparationService` execution steps.
- **Rationale**: Keeps state thread-safe and centralized without introducing external messaging dependencies.

### Decision 2: Expose status via a dedicated endpoint
- **Choice**: Add an HTTP GET action (e.g., `/api/ai/status` or `HomeController.Status`) returning `{ isReady, stage, progress, message, provider }`.
- **Rationale**: Allows client-side JavaScript on `Index.cshtml` to perform periodic polling (every 2-3 seconds) while the model is actively preparing.

### Decision 3: Reactive UI integration in `Index.cshtml`
- **Choice**: Render a dedicated warning banner component in `Index.cshtml` (server-rendered if model is not yet ready at initial load) and manage `analyzeButton` disabled state based on the combined condition: `hasValidFile && isModelReady`.
- **Rationale**: Prevents form submission during downloads while giving the user live feedback without requiring a full page refresh.

## Risks / Trade-offs

- **[Risk]** Polling overhead while downloading large models over several minutes.
  → **Mitigation**: Poll at a conservative interval (2.5 - 3 seconds) and stop polling immediately once the model reports `Ready` or `Faulted`.
- **[Risk]** Multiple browser tabs or refreshed pages.
  → **Mitigation**: Server-rendered markup reflects current state upon initial page load, and the polling loop synchronizes the UI automatically.

## Migration Plan

1. Update `FoundryLocalModelState` to track download progress and preparation stages.
2. Update `FoundryLocalModelPreparationService` to report progress updates into `FoundryLocalModelState`.
3. Add a model status endpoint on `HomeController`.
4. Update `Index.cshtml` view and script to display the warning banner, poll for readiness, and coordinate button disable/enable states.
5. Add unit tests for status reporting and endpoint responses.
