## MODIFIED Requirements

### Requirement: Foundry Local Model Availability
The system SHALL prepare the default Foundry Local model in the background without blocking application startup by awaiting model download completion before loading into memory, SHALL report model preparation status (downloading/loading progress, readiness, or fault) to the user interface, SHALL display a warning and disable proposal analysis submission while the model is downloading or preparing, and SHALL reject proposal analysis requests with a clear error until the model finishes preparing or report a clear error if preparation failed.

#### Scenario: Analysis attempted while model is still preparing
- **GIVEN** the Foundry Local provider is selected and the configured model has not finished downloading and loading
- **WHEN** a user submits a proposal for analysis
- **THEN** the system rejects the request and reports that the AI model is still preparing, without crashing the application

#### Scenario: Analysis attempted after model preparation failed
- **GIVEN** the Foundry Local provider is selected and preparing or downloading the configured model failed
- **WHEN** a user submits a proposal for analysis
- **THEN** the system rejects the request and reports that the AI model failed to prepare

#### Scenario: Analysis succeeds once model is ready
- **GIVEN** the Foundry Local provider is selected and the configured model has finished downloading and loading
- **WHEN** a user submits a proposal for analysis
- **THEN** the system uses the prepared model to analyze the proposal

#### Scenario: Warning displayed and analyze button disabled during model download or preparation
- **GIVEN** the Foundry Local provider is selected and the model is actively downloading or loading in the background
- **WHEN** the user views the proposal analysis page
- **THEN** the user interface displays a warning banner indicating model preparation status and keeps the analyze proposal button disabled even if a file is selected

#### Scenario: Warning removed and analyze button enabled when model becomes ready
- **GIVEN** the proposal analysis page is displaying the model preparation warning
- **WHEN** background model preparation completes successfully and a valid proposal file is selected
- **THEN** the user interface clears or hides the preparation warning and enables the analyze proposal button

#### Scenario: Model preparation failure displayed in UI
- **GIVEN** the Foundry Local provider is selected and model downloading or preparation fails
- **WHEN** the user views or interacts with the proposal analysis page
- **THEN** the user interface displays an error banner indicating that the model failed to prepare and keeps analysis submission disabled
