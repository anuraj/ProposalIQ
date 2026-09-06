## ADDED Requirements

### Requirement: Interactive AI Provider Management UI
The system SHALL provide a web settings interface where users can view the current AI provider configuration, switch between supported AI providers (`FoundryLocal`, `OpenRouter`, `OpenAI`, `AzureOpenAI`, `Local`), edit provider-specific models, endpoints, API keys, and model aliases, and save the updated configuration to persistent storage without restarting the application.

#### Scenario: User views current AI provider configuration
- **GIVEN** an active AI provider is configured in the application
- **WHEN** the user navigates to the AI provider settings page
- **THEN** the system displays the currently active provider and populates the configuration fields with the active settings

#### Scenario: User switches active provider and saves configuration
- **GIVEN** the user is on the AI provider settings page
- **WHEN** the user selects a different provider, enters valid models, endpoints, or credentials, and submits the form
- **THEN** the system saves the updated configuration, reconfigures the active AI chat client for subsequent analyses, and displays a success confirmation message

#### Scenario: Validation fails on missing required settings
- **GIVEN** the user selects a provider that requires specific parameters (such as an API key for OpenAI or endpoint for AzureOpenAI)
- **WHEN** the user attempts to save with required fields left blank
- **THEN** the system rejects the update, retains the entered inputs, and displays validation errors identifying the missing required fields

#### Scenario: Switching to Foundry Local initiates background model preparation
- **GIVEN** the application is currently using a cloud provider (such as OpenAI or OpenRouter)
- **WHEN** the user changes the active provider to `FoundryLocal` with a configured model alias and saves
- **THEN** the system initiates background model preparation for the configured Foundry Local model alias and updates the model readiness status in the user interface
