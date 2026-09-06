## 1. Project Dependencies & Configuration

- [x] 1.1 Add `Microsoft.Data.Sqlite` package reference to `Directory.Packages.props` and `Src/ProposalIQ.Web.csproj`
- [x] 1.2 Add SQLite cache configuration options (e.g., connection string or database file path) in `appsettings.json` and options classes

## 2. Cache Service Implementation

- [x] 2.1 Define `IProposalAnalysisCache` interface supporting asynchronous result retrieval and storage
- [x] 2.2 Implement `SqliteProposalAnalysisCache` with automatic table initialization, deterministic SHA-256 key computation, and JSON serialization
- [x] 2.3 Ensure `SqliteProposalAnalysisCache` handles database exceptions safely with warning logging and graceful fallback

## 3. Analysis Pipeline Integration

- [x] 3.1 Update `ProposalAnalysisService` to query `IProposalAnalysisCache` before invoking `_chatClient`
- [x] 3.2 Save generated `ProposalAnalysisResult` to `IProposalAnalysisCache` upon successful AI completion
- [x] 3.3 Register `IProposalAnalysisCache` in `Program.cs` with dependency injection

## 4. Testing & Verification

- [x] 4.1 Add unit tests for deterministic cache key generation and `SqliteProposalAnalysisCache` lifecycle
- [x] 4.2 Add unit tests for `ProposalAnalysisService` verifying cache hit avoids AI invocation and cache miss stores result
- [x] 4.3 Add unit tests verifying analysis continues without failure if cache operations encounter an exception
- [x] 4.4 Run `dotnet build` and `dotnet test` to verify clean build and all test suites passing
