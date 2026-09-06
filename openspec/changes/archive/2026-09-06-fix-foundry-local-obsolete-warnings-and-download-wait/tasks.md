## 1. Modernize Foundry Local State and Client

- [x] 1.1 Refactor `FoundryLocalModelState` to store initialized model and session factory references without obsolete types
- [x] 1.2 Update `FoundryLocalChatClient` to use `ChatSession`, map `Microsoft.Extensions.AI.ChatMessage` to `MessageItem`, and extract response content
- [x] 1.3 Ensure streaming responses in `FoundryLocalChatClient` use `ProcessStreamingRequestAsync` on `ChatSession`

## 2. Model Preparation Download Waiting and Progress Logging

- [x] 2.1 Update `FoundryLocalModelPreparationService` to pass `stoppingToken` into catalog and model operations
- [x] 2.2 Add download progress callback action to `model.DownloadAsync` with informative progress logging
- [x] 2.3 Explicitly await `model.DownloadAsync` completion prior to invoking `model.LoadAsync` and marking model state ready

## 3. Unit Testing and Verification

- [x] 3.1 Update `FoundryLocalModelStateTests` and `FoundryLocalChatClientTests` for the non-deprecated session model
- [x] 3.2 Add test cases verifying state transition behavior during download/loading and error handling
- [x] 3.3 Run `dotnet build` and `dotnet test` to verify zero compiler warnings and all tests passing
