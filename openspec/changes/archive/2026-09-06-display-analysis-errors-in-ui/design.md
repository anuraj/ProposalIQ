## Context

See [proposal.md](proposal.md) for background and motivation.

Currently, when `HomeController.Analyze` catches an error or validation issue (e.g. `ex.Message`, no readable text, missing file), it executes:
```csharp
ModelState.AddModelError("ProposalFile", $"Analysis failed: {ex.Message}");
return View("Index", request);
```
However, in `Index.cshtml`:
1. There is no server-rendered validation summary or `asp-validation-for="ProposalFile"` element rendered on page load.
2. `ViewBag.ModelStatus` is only populated in `HomeController.Index()`, so when returning `View("Index", request)` from `Analyze`, `ViewBag.ModelStatus` is null.
3. Client-side JavaScript sets `#uploadError` dynamically on drag/drop/selection errors, but does not render server `ModelState` errors.

## Goals / Non-Goals

**Goals:**
- Render server-side `ModelState` validation errors and analysis exception messages prominently in `Index.cshtml` using a clear Bootstrap alert banner.
- Populate `ViewBag.ModelStatus` in all `Analyze` exit paths returning the index view so the AI model readiness widget remains accurate.
- Retain user-entered deal context values in the input fields when an error occurs so the user doesn't lose their input.
- Seamlessly coordinate client-side file selection error clearing with server-rendered error banners.

**Non-Goals:**
- Altering the error view (`Error.cshtml`) used for unhandled 500 errors.
- Changing `ProposalAnalysisResult` schema.

## Decisions

### Decision 1: Render dedicated Server Error Alert Banner in `Index.cshtml`
- **Choice**: Add an `@if (!ViewData.ModelState.IsValid)` block above the analyzer card rendering a styled `alert alert-danger` containing the specific error message(s) from `ModelState`.
- **Rationale**: Gives immediate, unmistakable feedback when an analysis fails or text extraction returns empty, instead of silently failing.

### Decision 2: Centralize `ViewBag.ModelStatus` population in `HomeController`
- **Choice**: Ensure `GetCurrentModelStatus()` is called and assigned to `ViewBag.ModelStatus` before returning `View("Index", request)` in `Analyze`.
- **Rationale**: Prevents UI flicker or incorrect assumption that the model is ready or missing when returning from a failed analysis request.

### Decision 3: Client-side coordination
- **Choice**: When a new file is dropped or selected in the upload zone, if a server error banner is visible, dismiss or clear it to indicate a fresh attempt.
- **Rationale**: Improves UX by clearing stale server errors when the user fixes their input.

## Risks / Trade-offs

- **[Risk]** Error messages might expose raw internal technical exceptions.
  → **Mitigation**: Format friendly user-facing messages for known errors (empty text, cancelled requests, model preparation failures) while still providing the actionable message for analysis failures.

## Migration Plan

1. Update `HomeController.Analyze` to populate `ViewBag.ModelStatus = GetCurrentModelStatus()` before returning `View("Index", request)`.
2. Update `Index.cshtml` to render a server error banner when `!ViewData.ModelState.IsValid`.
3. Update `Index.cshtml` JavaScript to clear or hide server error alerts upon new file selection.
4. Add unit tests verifying `Analyze` adds model errors and populates `ViewBag.ModelStatus` on various error scenarios.
