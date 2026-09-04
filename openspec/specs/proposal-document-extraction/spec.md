# Proposal Document Extraction Specification

## Purpose

Defines how ProposalIQ accepts proposal document uploads and extracts readable text so supported proposal artifacts can be analyzed consistently.

## Requirements

### Requirement: Supported proposal document formats
The system SHALL accept `.txt`, `.docx`, `.pdf`, and `.pptx` proposal files for analysis and SHALL reject unsupported file types with a clear validation error.

#### Scenario: User selects a PowerPoint proposal
- **GIVEN** a user is on the proposal upload page
- **WHEN** the user selects a `.pptx` proposal file within the configured upload size limit
- **THEN** the system accepts the file as eligible for analysis

#### Scenario: User selects an existing supported format
- **GIVEN** a user is on the proposal upload page
- **WHEN** the user selects a `.txt`, `.docx`, or `.pdf` proposal file within the configured upload size limit
- **THEN** the system continues to accept the file as eligible for analysis

#### Scenario: User selects an unsupported format
- **GIVEN** a user is on the proposal upload page
- **WHEN** the user selects a file whose extension is not `.txt`, `.docx`, `.pdf`, or `.pptx`
- **THEN** the system rejects the file and tells the user which proposal file formats are supported

### Requirement: PowerPoint text extraction
The system SHALL extract readable text from supported PowerPoint proposal files and submit that text to the existing proposal analysis workflow.

#### Scenario: Analyze PowerPoint with slide text
- **GIVEN** a `.pptx` proposal file contains readable text on one or more slides
- **WHEN** the user submits the proposal for analysis
- **THEN** the system analyzes the extracted slide text using the existing proposal analysis workflow

#### Scenario: PowerPoint has no readable text
- **GIVEN** a `.pptx` proposal file contains no readable text
- **WHEN** the user submits the proposal for analysis
- **THEN** the system does not call proposal analysis and tells the user that no readable text was found in the proposal