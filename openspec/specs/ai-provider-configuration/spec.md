# AI Provider Configuration

## Purpose

Defines how ProposalIQ selects and validates the configured AI chat provider used for proposal analysis.

## Requirements

### Requirement: Configurable AI Provider Selection
The system SHALL select the AI chat provider used for proposal analysis from configuration, defaulting to Foundry Local when no provider is explicitly configured.

#### Scenario: Default provider is Foundry Local
- **GIVEN** no `Ai:Provider` configuration value is set
- **WHEN** the application starts
- **THEN** the system selects Foundry Local as the AI provider and begins preparing the configured model for use, without requiring an API key

#### Scenario: Default local model is Qwen-family
- **GIVEN** no `Ai:Provider` and no `Ai:FoundryLocal:ModelAlias` configuration values are set
- **WHEN** the application starts
- **THEN** the system prepares a Qwen-family model id suitable for Foundry Local

#### Scenario: Provider explicitly set to FoundryLocal
- **GIVEN** `Ai:Provider` is set to `FoundryLocal` and a valid `Ai:FoundryLocal:ModelAlias` is configured
- **WHEN** the application starts
- **THEN** the system selects Foundry Local as the AI provider and begins preparing the configured model for use, without requiring an API key

#### Scenario: Default provider is OpenRouter
- **GIVEN** `Ai:Provider` is set to `OpenRouter` and a valid `Ai:OpenRouter:ApiKey`, `Ai:OpenRouter:Endpoint`, and `Ai:OpenRouter:Model` are configured
- **WHEN** the application starts
- **THEN** the system configures the chat client to call OpenRouter using the configured endpoint, model, and API key

#### Scenario: Provider explicitly set to OpenAI
- **GIVEN** `Ai:Provider` is set to `OpenAI` and a valid `Ai:OpenAI:ApiKey` and `Ai:OpenAI:Model` are configured
- **WHEN** the application starts
- **THEN** the system configures the chat client to call the OpenAI API using the configured model and API key

#### Scenario: Provider explicitly set to AzureOpenAI
- **GIVEN** `Ai:Provider` is set to `AzureOpenAI` and valid `Ai:AzureOpenAI:Endpoint`, `Ai:AzureOpenAI:Deployment`, and `Ai:AzureOpenAI:ApiKey` are configured
- **WHEN** the application starts
- **THEN** the system configures the chat client to call the specified Azure OpenAI deployment at the specified endpoint

#### Scenario: Provider explicitly set to Local
- **GIVEN** `Ai:Provider` is set to `Local` and a valid `Ai:Local:Endpoint` and `Ai:Local:Model` are configured
- **WHEN** the application starts
- **THEN** the system configures the chat client to call the local OpenAI-compatible endpoint using the configured model, without requiring an API key

### Requirement: Fail-fast Configuration Validation
The system SHALL validate the required settings for the selected provider at startup and throw a clear, actionable error before the host starts if any required setting is missing.

#### Scenario: Missing API key for selected provider
- **GIVEN** `Ai:Provider` is set to `OpenAI` and `Ai:OpenAI:ApiKey` is missing or blank
- **WHEN** the application starts
- **THEN** the system throws an `InvalidOperationException` identifying the missing setting name before the host runs

#### Scenario: Unknown provider value
- **GIVEN** `Ai:Provider` is set to a value that does not match any supported provider
- **WHEN** the application starts
- **THEN** the system throws an `InvalidOperationException` listing the supported provider values

#### Scenario: Default Foundry Local settings are present
- **GIVEN** no AI provider configuration values are set
- **WHEN** the application starts
- **THEN** the system does not throw a missing model configuration error for the default Foundry Local provider

#### Scenario: Missing Foundry Local model alias
- **GIVEN** `Ai:Provider` is set to `FoundryLocal` and `Ai:FoundryLocal:ModelAlias` is missing or blank
- **WHEN** the application starts
- **THEN** the system throws an `InvalidOperationException` identifying the missing setting name before the host runs

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
