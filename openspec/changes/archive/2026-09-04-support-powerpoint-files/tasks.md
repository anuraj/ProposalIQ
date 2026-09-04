## 1. Extraction Tests

- [x] 1.1 Add focused tests for `.pptx` extraction with readable slide text and verify the new tests fail before implementation.
- [x] 1.2 Add focused tests for `.pptx` files with no readable slide text and verify the existing empty-text handling remains observable.
- [x] 1.3 Add or update unsupported-extension tests so non-supported formats are rejected and verify `.txt`, `.docx`, `.pdf`, and `.pptx` are the only accepted extensions.

## 2. PowerPoint Extraction

- [x] 2.1 Add `.pptx` handling to the proposal text extraction service and verify the readable-slide-text extraction test passes.
- [x] 2.2 Extract slide text in presentation order with stable line breaks and verify multi-slide test content appears in the expected order.
- [x] 2.3 Preserve existing `.txt`, `.docx`, and `.pdf` extraction paths and verify existing extraction or analysis tests still pass.

## 3. Upload UI

- [x] 3.1 Update the upload input `accept` list and displayed format text to include PowerPoint and verify the page renders `.pptx` as a supported format.
- [x] 3.2 Update client-side allowed extensions and validation error text to include `.pptx` and verify selecting a `.pptx` file enables analysis while unsupported formats show the supported-format error.

## 4. Validation

- [x] 4.1 Run `dotnet test` from `Tests/` and verify the full test suite passes.
- [x] 4.2 Run `openspec validate support-powerpoint-files --strict` and verify the change passes OpenSpec validation.