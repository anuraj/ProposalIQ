# Design: Configure AI Provider

## Context

`Program.cs` currently builds a single `ChatClient` pointed at OpenRouter and registers it via `AddChatClient(...).AsIChatClient()`. All other code (`ProposalAnalysisService`) depends on the `IChatClient` abstraction from `Microsoft.Extensions.AI`, so no downstream code needs to change — only how the `IChatClient` is constructed.

## Goals

- Select provider construction logic from configuration, keeping `IChatClient` as the only abstraction consumers see.
- Keep OpenRouter as the zero-config default so `OpenRouter:ApiKey` in existing user secrets keeps working.
- Fail fast with a clear message when the selected provider is missing required settings.

## Configuration Shape

```jsonc
// appsettings.json (placeholders only; real secrets via user-secrets/env vars)
{
  "Ai": {
    "Provider": "OpenRouter", // OpenRouter | OpenAI | AzureOpenAI | Local
    "OpenRouter": {
      "Model": "openrouter/auto",
      "Endpoint": "https://openrouter.ai/api/v1/",
      "ApiKey": ""
    },
    "OpenAI": {
      "Model": "gpt-4o-mini",
      "ApiKey": ""
    },
    "AzureOpenAI": {
      "Endpoint": "",
      "Deployment": "",
      "ApiKey": ""
    },
    "Local": {
      "Model": "",
      "Endpoint": "http://localhost:11434/v1/"
    }
  }
}
```

`Ai:OpenRouter:ApiKey` continues to fall back to the legacy `OpenRouter:ApiKey` key for backward compatibility (read the legacy key if the nested one is blank), so existing user-secrets setups keep working without edits.

## Provider Options and Factory

Add `Src/Configuration/AiProviderOptions.cs` (POCO bound from `Ai`) with nested option types (`OpenRouterOptions`, `OpenAiOptions`, `AzureOpenAiOptions`, `LocalOptions`) and an `AiProvider` enum (`OpenRouter`, `OpenAI`, `AzureOpenAI`, `Local`).

Add `Src/Services/ChatClientFactory.cs` (static or small class) with a single method:

```csharp
public static IChatClient Create(AiProviderOptions options)
```

Behavior:
- Switches on `options.Provider` (case-insensitive enum parse; unknown value throws `InvalidOperationException` listing valid values).
- `OpenRouter` / `Local`: builds `OpenAIClient` with `ApiKeyCredential` (empty key allowed for `Local`) and a custom `Endpoint`, then `.GetChatClient(model).AsIChatClient()`.
- `OpenAI`: builds `OpenAIClient` with the configured API key and default endpoint, then `.GetChatClient(model).AsIChatClient()`.
- `AzureOpenAI`: builds `AzureOpenAIClient` (from the new `Azure.AI.OpenAI` package) using `Endpoint` + `ApiKeyCredential`, then `.GetChatClient(deployment).AsIChatClient()`.
- Before building, validates required fields for the selected provider are non-blank; throws `InvalidOperationException` naming the missing configuration key.

`Program.cs` changes to:

```csharp
var aiOptions = builder.Configuration.GetSection("Ai").Get<AiProviderOptions>() ?? new AiProviderOptions();
// legacy fallback for OpenRouter:ApiKey
if (string.IsNullOrWhiteSpace(aiOptions.OpenRouter.ApiKey))
{
    aiOptions.OpenRouter.ApiKey = builder.Configuration["OpenRouter:ApiKey"] ?? string.Empty;
}

var chatClient = ChatClientFactory.Create(aiOptions);
builder.Services.AddChatClient(chatClient);
```

## Alternatives Considered

- **Keyed DI registrations per provider** (`AddKeyedChatClient`): rejected as unnecessary complexity — only one provider is active per running instance, so a factory chosen once at startup is simpler than keyed resolution.
- **`IOptions<AiProviderOptions>` with a hosted validation step**: considered for validation, but since the `IChatClient` must be constructed before `builder.Build()` (registered as a singleton instance), validation happens inline in the factory instead of via `IValidateOptions`.

## Risks

- Azure OpenAI SDK surface differs slightly across versions; pin `Azure.AI.OpenAI` version compatible with `Microsoft.Extensions.AI.OpenAI` 10.9.0 and verify `AsIChatClient()` extension resolves for both client types.
- Local/OpenAI-compatible servers may not support every OpenAI Chat Completions option `ProposalAnalysisService` relies on (e.g. structured output) — out of scope for this change; document as a known limitation.
