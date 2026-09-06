## Why

Re-analyzing previously processed proposal documents with identical deal parameters triggers repetitive, costly, and time-consuming AI model invocations. Implementing local persistent caching with SQLite allows ProposalIQ to immediately return cached analysis results on duplicate uploads, improving response times and saving compute/token resources.

## What Changes

- Introduce a persistent proposal analysis result cache backed by SQLite (`Microsoft.Data.Sqlite`).
- Compute a deterministic SHA-256 cache key based on the extracted proposal text (or document contents) and deal context parameters (project value, target hourly rate, additional context).
- Check the cache in the analysis pipeline before invoking the underlying AI `IChatClient`; if a matching cached result exists, deserialize and return it immediately.
- Store newly generated analysis results asynchronously in SQLite upon successful AI analysis.
- Provide configuration options to configure the SQLite cache database path or enable/disable caching.

## Capabilities

### New Capabilities
- `proposal-analysis-caching`: Defines requirements for caching and retrieving proposal risk analysis results based on document content hash and deal context.

### Modified Capabilities
<!-- None -->

## Non-goals

- Implementing distributed or cloud-hosted database caching (e.g. Redis, SQL Server).
- Modifying prompt analysis instructions or the structure of `ProposalAnalysisResult`.
- Providing manual cache invalidation UI or administrative dashboards in this change.

## Impact

- Adds `Microsoft.Data.Sqlite` dependency.
- Adds `IProposalAnalysisCache` service interface and `SqliteProposalAnalysisCache` implementation.
- Updates `ProposalAnalysisService` or controller pipeline to consult the cache.
- Configures SQLite database file location in `appsettings.json` / `Program.cs`.
- Adds unit and integration tests verifying cache hit, cache miss, deterministic hashing, and fallback behavior on database errors.
