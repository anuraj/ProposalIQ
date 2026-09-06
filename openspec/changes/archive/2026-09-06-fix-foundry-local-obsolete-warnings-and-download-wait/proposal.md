## Why

The current Foundry Local integration uses deprecated `OpenAIChatClient` and `Model.GetChatClientAsync` APIs scheduled for removal, generating deprecation warnings. Additionally, background model preparation must reliably wait for model downloads to complete across large model variants with progress reporting and cancellation handling before loading the model for inference.

## What Changes

- Replace deprecated `Microsoft.AI.Foundry.Local.OpenAIChatClient` and `Model.GetChatClientAsync` usage with `ChatSession` and native Foundry Local request/response item models in `FoundryLocalChatClient` and `FoundryLocalModelState`.
- Ensure `FoundryLocalModelPreparationService` explicitly waits for `model.DownloadAsync(...)` to complete with progress logging and `CancellationToken` support prior to invoking `model.LoadAsync(...)`.
- Update `FoundryLocalModelState` and unit tests to verify `ChatSession`-based execution and robust download readiness/fault handling without compiler deprecation warnings.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `ai-provider-configuration`: Update Foundry Local model preparation and client execution requirements to ensure download completion tracking, cancellation responsiveness, and non-deprecated session-based chat processing.

## Non-goals

- Adding new AI provider types or altering existing OpenAI / OpenRouter / Azure OpenAI providers.
- Changing the web UI or user-facing analysis output structure.
- Changing how proposal document text extraction functions.

## Impact

- `Src/Services/FoundryLocalChatClient.cs`: Migrated from Betalgo/OpenAIChatClient to `ChatSession` and `Microsoft.AI.Foundry.Local` items.
- `Src/Services/FoundryLocalModelState.cs`: Stores model / session provider references cleanly without obsolete types.
- `Src/Services/FoundryLocalModelPreparationService.cs`: Explicitly awaits download completion with progress callback and cancellation token.
- `Tests/`: Updated Foundry Local test suite to reflect updated state and client models.
