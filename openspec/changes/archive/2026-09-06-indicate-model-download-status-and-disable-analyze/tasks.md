## 1. Status Tracking & State Model

- [x] 1.1 Add preparation stages (e.g. `Downloading`, `Loading`, `Ready`, `Faulted`) and progress tracking to `FoundryLocalModelState`
- [x] 1.2 Update `FoundryLocalModelPreparationService` to record preparation stage transitions and download progress percentage into `FoundryLocalModelState`

## 2. API Endpoint & Controller Status Action

- [x] 2.1 Add a status model and endpoint/action in `HomeController` returning AI provider readiness and download progress
- [x] 2.2 Update `HomeController.Index` to provide initial AI readiness status to the view

## 3. UI Warning Banner & Reactive Action Button

- [x] 3.1 Add a model download/preparation warning banner component in `Index.cshtml`
- [x] 3.2 Update client-side logic in `Index.cshtml` to poll model status while downloading/preparing, update warning message/progress, and handle error states
- [x] 3.3 Ensure the "Analyze Proposal" button remains disabled whenever the model is preparing, and enables dynamically once ready and a file is selected

## 4. Testing & Verification

- [x] 4.1 Add unit tests for `FoundryLocalModelState` status transitions and progress recording
- [x] 4.2 Add controller unit tests verifying status action response across different provider and readiness states
- [x] 4.3 Run `dotnet build` and `dotnet test` to verify zero errors and all tests passing
