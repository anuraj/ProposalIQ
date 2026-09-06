## Why

ProposalIQ currently defaults to OpenRouter, which requires a hosted API key before local development or evaluation can run. Foundry Local provides an on-machine OpenAI-compatible runtime, making the default experience cheaper, private by default, and easier to start without external AI service setup.

## What Changes

- Change the default AI provider from OpenRouter to Foundry Local through the existing local OpenAI-compatible provider path.
- Set the default local provider endpoint to the Foundry Local OpenAI-compatible endpoint.
- Set the default local model to a Qwen-family model, with the exact model id remaining configurable.
- Preserve explicit configuration support for OpenRouter, OpenAI, Azure OpenAI, and local OpenAI-compatible providers.
- Keep fail-fast validation for required provider settings.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `ai-provider-configuration`: Default provider selection and local provider defaults change from OpenRouter-first to Foundry Local-first.

## Non-goals

- Removing OpenRouter, OpenAI, Azure OpenAI, or generic local OpenAI-compatible provider support.
- Bundling, installing, starting, or managing the Foundry Local runtime.
- Hardcoding secrets or committing API keys.
- Changing proposal analysis prompts, result models, or document extraction behavior.

## Impact

- Affects AI provider option defaults, application configuration, startup validation behavior, and provider selection tests.
- Documentation or sample configuration may need to describe how to run Foundry Local and override the default model or endpoint.
- Assumption: the default model should be a configurable Qwen-family model id suitable for Foundry Local; implementation should use the currently supported Foundry model identifier if it differs from plain `qwen`.