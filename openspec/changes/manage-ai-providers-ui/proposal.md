## Why

Currently, AI provider selection and credentials (models, endpoints, API keys, and model aliases) can only be changed by editing `appsettings.json` or `.NET` user secrets before starting the application. Providing an interactive Web UI for configuring AI providers, models, and endpoints allows users to switch providers, adjust endpoints and models, update API keys, and trigger local model preparation directly from the browser without restarting the server or manually editing configuration files.

## What Changes

- Add a dedicated Settings / Provider Management view (`/Settings` or `/Settings/Index`) accessible from the main navigation header.
- Provide forms and controls to switch the active AI provider (`FoundryLocal`, `OpenRouter`, `OpenAI`, `AzureOpenAI`, `Local`) and manage provider-specific options (model names, endpoints, API keys, and Foundry Local model aliases).
- Introduce a persistent runtime AI configuration provider/store that persists updated settings to durable local storage (e.g. `appsettings.local.json` or SQLite) and dynamically refreshes the registered `IChatClient` and background preparation services.
- Validate submitted configuration in real-time (checking required non-blank fields and endpoint formats) with friendly UI feedback and success notifications.
- Mask sensitive API keys in the UI with an option to update or replace them.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `ai-provider-configuration`: Update requirements to support interactive runtime AI provider management, model/endpoint configuration updates via UI, persistent runtime settings storage, and dynamic chat client reconfiguration.

## Non-goals

- Implementing multi-tenant user access control or role-based permission management for the settings page.
- Adding arbitrary custom AI provider plugins outside the supported provider types (`FoundryLocal`, `OpenRouter`, `OpenAI`, `AzureOpenAI`, `Local`).

## Impact

- Adds `SettingsController` with GET and POST actions for viewing and saving AI provider settings.
- Adds `Views/Settings/Index.cshtml` UI with responsive provider selection and field editing.
- Updates navigation header in `_Layout.cshtml` to include a Settings link.
- Introduces dynamic `IChatClientProvider` / reloadable `AiProviderOptions` service.
- Adds unit tests for `SettingsController` and runtime provider reconfiguration.
