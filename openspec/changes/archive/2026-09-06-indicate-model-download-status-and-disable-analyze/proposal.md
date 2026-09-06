## Why

When the application uses Foundry Local, the AI model may take several minutes to download in the background on startup or first run. Without visible UI feedback, users upload documents and attempt analysis, only to encounter unexpected error alerts or failed requests.

## What Changes

- Expose AI model readiness status through an API endpoint or controller action (`/api/model-status` or home controller).
- Display a prominent, friendly warning and progress indicator on the proposal submission page (`Index.cshtml`) while the local model is downloading/preparing in the background.
- Keep the "Analyze Proposal" button disabled while the model is downloading or preparing, preventing premature submissions.
- Automatically enable the "Analyze Proposal" button once the model finishes preparing (and a valid proposal file is selected).
- Display a clear error alert if background model preparation encounters a failure.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `ai-provider-configuration`: Add requirements for communicating AI model preparation status to the UI, displaying warning state and progress while downloading/loading, and disabling proposal analysis actions until the model is ready.

## Non-goals

- Modifying the AI analysis prompt or document extraction pipeline.
- Altering the behavior of remote AI providers (OpenAI, Azure OpenAI, OpenRouter) that do not require local model downloads.
- Adding complex multi-stage background job queues.

## Impact

- `Src/Controllers/HomeController.cs`: Add status query endpoint/action for AI model preparation state.
- `Src/Views/Home/Index.cshtml`: Add warning banner component and client-side status polling logic to disable/enable the analyze button based on model readiness and file selection.
- `Src/Services/FoundryLocalModelState.cs` / `FoundryLocalModelPreparationService.cs`: Expose current preparation stage (e.g., downloading, loading, ready, faulted) and progress percentage if available.
- `Tests/`: Add unit tests for the model status controller action and state reporting.
