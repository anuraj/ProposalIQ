## MODIFIED Requirements

### Requirement: Foundry Local Model Availability
The system SHALL prepare the default Foundry Local model in the background without blocking application startup by awaiting model download completion before loading into memory, and SHALL reject proposal analysis requests with a clear error until the model finishes preparing or report a clear error if preparation failed.

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
