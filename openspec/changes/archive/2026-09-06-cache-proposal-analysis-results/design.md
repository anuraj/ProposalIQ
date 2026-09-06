## Context

See [proposal.md](proposal.md) for background and motivation.

Currently, every proposal submission runs through `HomeController.Analyze`, calling `IProposalAnalysisService.AnalyzeAsync`. For large documents or local AI providers (like Foundry Local) where inference latency can take several seconds to minutes, re-evaluating the exact same document and deal context wastes time and compute resources.

## Goals / Non-Goals

**Goals:**
- Provide persistent, local caching of proposal analysis results backed by SQLite (`Microsoft.Data.Sqlite`).
- Hash extracted proposal text and deal context parameters into a deterministic SHA-256 cache key.
- Check cache before executing `IChatClient` completions and immediately return stored `ProposalAnalysisResult` on a cache hit.
- Asynchronously persist newly evaluated analysis results to SQLite upon completion.
- Ensure analysis requests continue successfully even if SQLite encounters IO, lock, or disk errors (graceful fallback).

**Non-Goals:**
- Distributed cache backends (Redis, Memcached, cloud database).
- Multi-user authentication or per-user cache partitions.
- Cache TTL expiration or LRU eviction policies in this initial change (database size can be kept simple and durable).

## Decisions

### Decision 1: Use `Microsoft.Data.Sqlite` for local persistent storage
- **Choice**: Use lightweight ADO.NET SQLite (`Microsoft.Data.Sqlite`) with automatic table initialization (`CREATE TABLE IF NOT EXISTS ProposalAnalysisCache (...)`).
- **Rationale**: SQLite requires zero separate server setup, produces a portable single-file database (`Data/proposaliq.db` or configured path), and offers high-performance local read/write operations in ASP.NET Core applications.
- **Alternatives Considered**:
  - `Microsoft.Extensions.Caching.Memory`: In-memory only, lost on every server restart.
  - Entity Framework Core with SQLite: Excessive weight and migrations overhead for a single simple key-value table.

### Decision 2: Cache Key Computation
- **Choice**: Compute a SHA-256 hash over normalized concatenated inputs:
  - Extracted proposal text (trimmed)
  - Normalized ProjectValue (`{ProjectValue:F2}` or empty)
  - Normalized HourlyRate (`{HourlyRate:F2}` or empty)
  - AdditionalContext (trimmed or empty)
- **Rationale**: Ensures exact matching for duplicate proposals and deal scenarios while properly detecting changes in context or document text.

### Decision 3: Service Layer Architecture
- **Choice**: Introduce `IProposalAnalysisCache` interface with `GetAsync(string cacheKey, CancellationToken ct)` and `SetAsync(string cacheKey, ProposalAnalysisResult result, CancellationToken ct)`. Inject this cache into `ProposalAnalysisService` (or decorate it).
- **Rationale**: Keeps database access decoupled from AI analysis logic, allows straightforward mocking and unit testing, and facilitates future cache store implementations if needed.

### Decision 4: Serialization and Schema
- **Choice**: Store results in table `AnalysisCache` with columns: `CacheKey` (TEXT PRIMARY KEY), `ResultJson` (TEXT NOT NULL), `CreatedAtUtc` (TEXT NOT NULL).
- **Rationale**: Serializing `ProposalAnalysisResult` directly to JSON via `System.Text.Json` guarantees complete fidelity of the analysis object tree (risks, summaries, and issues) without complex relational mapping.

## Risks / Trade-offs

- **[Risk]** SQLite concurrency or database lock errors under concurrent requests.
  → **Mitigation**: Use WAL (Write-Ahead Logging) mode and wrap cache operations in try/catch blocks that log warnings and transparently fall back to live AI execution on database exceptions.
- **[Risk]** Stale or outdated analysis results if AI prompts are updated in the codebase.
  → **Mitigation**: Future changes can include prompt versioning in the hash key; for now, deleting the local `.db` file or updating deal context easily bypasses cache.

## Migration Plan

1. Add `Microsoft.Data.Sqlite` package reference to `Src/ProposalIQ.Web.csproj` and `Directory.Packages.props`.
2. Implement `IProposalAnalysisCache` and `SqliteProposalAnalysisCache`.
3. Update `ProposalAnalysisService` to check the cache before calling `_chatClient.GetResponseAsync<ProposalAnalysisResult>` and save upon completion.
4. Register the cache service in `Program.cs`.
5. Add unit and integration tests verifying cache hit, cache miss, deterministic hashing, and error resilience.
