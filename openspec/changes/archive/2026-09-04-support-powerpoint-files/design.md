## Context

See proposal.md for motivation. The current upload path accepts `.txt`, `.docx`, and `.pdf` files in the Razor page and extracts text through the existing proposal text extraction service before invoking analysis. Word extraction already uses Open XML, and PDF extraction uses PdfPig.

## Goals / Non-Goals

**Goals:**
- Add `.pptx` as a first-class supported upload format without changing the controller or analysis service contract.
- Extract readable slide text in a deterministic order suitable for the existing analysis prompt.
- Keep existing `.txt`, `.docx`, and `.pdf` behavior unchanged.
- Cover extraction and validation behavior with focused tests.

**Non-Goals:**
- Support legacy `.ppt` binary files.
- Interpret visual layout, charts, speaker notes, images, embedded documents, or animations.
- Change upload size limits, AI model configuration, or analysis result models.

## Decisions

### Use the existing extraction service boundary

PowerPoint handling will be added inside the proposal text extraction service, keyed by `.pptx`, while the controller continues to validate only presence, empty extracted text, and analysis flow.

Rationale: the service already owns format-specific parsing, so adding a new parser there preserves the current MVC shape and keeps the controller thin.

Alternatives considered:
- Add PowerPoint logic in the controller. Rejected because it would duplicate format-specific extraction responsibilities outside the service.
- Create separate extractor classes per file type. Deferred because there are only four supported formats and no current abstraction pressure requiring a larger refactor.

### Parse `.pptx` with Open XML presentation parts

PowerPoint text extraction will use the existing DocumentFormat.OpenXml dependency by opening the package as a presentation document, iterating slides in presentation order, and collecting non-empty text runs from slide content.

Rationale: `.pptx` is an Open XML format, and the project already depends on Open XML for `.docx`. Reusing it avoids a new external dependency and keeps document parsing consistent.

Alternatives considered:
- Add a PowerPoint-specific parser package. Rejected unless Open XML proves insufficient for basic readable text extraction.
- Convert slides to another format before extraction. Rejected because conversion adds operational complexity and is unnecessary for text-only analysis.

### Update upload affordances in one place

The upload view should include `.pptx` in the file input `accept` list, client-side allowed extensions, displayed format list, and unsupported-file error message.

Rationale: users should see and receive consistent guidance before submission. Server-side extraction remains the source of truth for supported extensions.

Alternatives considered:
- Server-only support. Rejected because users would still be blocked by client-side validation and the browser file picker hint.

## Risks / Trade-offs

- PowerPoint files with text embedded only in images will extract as empty text -> Existing empty-text handling reports that no readable text was found.
- Slide text ordering can affect analysis quality -> Iterate slides in presentation order and text elements in document order, separating slides/paragraphs with line breaks.
- Open XML packages can be malformed or encrypted -> Let parsing failures surface through the existing analysis failure path while tests cover normal and empty-readable-text cases.
- Client-side validation is bypassable -> Keep server-side unsupported-extension rejection in the extraction service.

## Migration Plan

No data migration is required. Deployment is a normal application update; rollback removes `.pptx` from the accepted client formats and the extraction service switch.