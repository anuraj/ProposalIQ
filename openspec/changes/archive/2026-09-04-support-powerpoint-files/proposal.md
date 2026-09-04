## Why

ProposalIQ currently accepts text, Word, and PDF proposals, but many sellers prepare proposal decks in PowerPoint. Supporting PowerPoint files removes a manual conversion step and lets users analyze the proposal artifacts they already send to prospects.

## What Changes

- Allow users to upload PowerPoint proposal files for analysis.
- Extract readable text from supported PowerPoint files and pass it through the existing analysis flow.
- Update client-side upload validation and user-facing format labels to include PowerPoint.
- Preserve existing behavior for `.txt`, `.docx`, and `.pdf` files.

## Capabilities

### New Capabilities
- `proposal-document-extraction`: Defines supported proposal document formats and text extraction behavior before analysis.

### Modified Capabilities
- None.

## Non-goals

- Supporting legacy binary `.ppt` files in this change.
- Extracting or analyzing images, speaker notes, embedded media, animations, charts, or slide layout semantics beyond readable slide text.
- Changing the AI risk analysis prompt, model provider configuration, or result schema.

## Impact

- Affects the proposal upload UI, client-side extension validation, and `ProposalTextExtractor`.
- Likely requires using the existing Open XML dependency to read `.pptx` slide text.
- Tests should cover PowerPoint extraction, unsupported file rejection, and preservation of existing supported formats.