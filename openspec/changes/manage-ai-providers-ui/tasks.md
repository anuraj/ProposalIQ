## 1. Dynamic Configuration & Client Architecture

- [x] 1.1 Create `IAiConfigurationService` interface and `JsonFileAiConfigurationService` to manage runtime `AiProviderOptions` and disk persistence
- [x] 1.2 Implement dynamic `IChatClient` delegating wrapper (`DynamicChatClient`) that resolves the active client based on current `AiProviderOptions`
- [x] 1.3 Update `Program.cs` to register dynamic configuration and dynamic `IChatClient` services

## 2. Settings Controller & Validation

- [x] 2.1 Create `SettingsViewModel` and `SettingsController` with `Index` GET and POST actions
- [x] 2.2 Implement server-side validation for required fields per provider and handle masked API keys
- [x] 2.3 Coordinate background model preparation when switching to `FoundryLocal`

## 3. Settings UI & Site Navigation

- [x] 3.1 Create `Views/Settings/Index.cshtml` with provider selection, configuration panels for each provider, and masked API keys
- [x] 3.2 Update `_Layout.cshtml` header to include a Settings navigation link
- [x] 3.3 Add client-side interactivity in `Index.cshtml` to toggle provider-specific setting sections dynamically

## 4. Testing & Verification

- [x] 4.1 Add unit tests for `JsonFileAiConfigurationService` and `DynamicChatClient`
- [x] 4.2 Add unit tests for `SettingsController` covering validation, provider updates, and API key preservation
- [x] 4.3 Run `dotnet build` and `dotnet test` to verify clean build and all test suites passing
