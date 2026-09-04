# AI Provider Configuration

## Purpose

Defines how ProposalIQ selects and validates the configured AI chat provider used for proposal analysis.

## Requirements

### Requirement: Configurable AI Provider Selection
The system SHALL select the AI chat provider used for proposal analysis from configuration instead of a hardcoded OpenRouter client.

#### Scenario: Default provider is OpenRouter
- **GIVEN** no `Ai:Provider` configuration value is set
- **WHEN** the application starts
- **THEN** the system configures the chat client to use OpenRouter, preserving current behavior

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
