## Why

When proposal text extraction or AI risk analysis encounters an error, exception, or validation failure during submission, the backend sets `ModelState` errors and re-renders the index view. However, because the Razor view lacks server-rendered error alert markup and does not preserve model status state on validation returns, failures appear silent or cause confusing UI states without actionable error messages.

## What Changes

- Add server-rendered error alert and validation summary presentation to `Src/Views/Home/Index.cshtml` to visibly surface extraction errors, AI model failures, empty proposal errors, and general analysis exceptions.
- Ensure `HomeController.Analyze` preserves necessary view data (such as `ViewBag.ModelStatus`) and form input state when returning the index view on failure.
- Ensure client-side upload handling interacts cleanly with server-rendered error banners (clearing errors on new file selection or displaying server-returned error text).
- Provide clear, user-friendly error banners and detail messages for specific error scenarios (e.g., corrupt document, unreadable text, AI provider timeout, model preparation failure).

## Capabilities

### New Capabilities
- `proposal-analysis-error-presentation`: Defines requirements for visibly surfacing proposal extraction and analysis errors, exceptions, and validation failures in the web interface.

### Modified Capabilities
<!-- None -->

## Non-goals

- Altering the core AI analysis prompts or schema in `ProposalAnalysisResult`.
- Modifying underlying PDF/DOCX/PPTX text extraction algorithms.
- Changing logging configuration.

## Impact

- `Src/Views/Home/Index.cshtml`: Render server validation/error alert banner prominently above the analyzer or upload zone.
- `Src/Controllers/HomeController.cs`: Ensure model status and error context are properly populated in `Analyze` error exit paths.
- `Tests/HomeControllerTests.cs`: Add tests verifying that `Analyze` returns appropriate model errors and view data on failures.
