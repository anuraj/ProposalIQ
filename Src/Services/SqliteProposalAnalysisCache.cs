using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Models;

namespace ProposalIQ.Web.Services;

public class SqliteProposalAnalysisCache : IProposalAnalysisCache
{
    private readonly AnalysisCacheOptions _options;
    private readonly ILogger<SqliteProposalAnalysisCache> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _isInitialized;

    public SqliteProposalAnalysisCache(
        IOptions<AnalysisCacheOptions> options,
        ILogger<SqliteProposalAnalysisCache> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string ComputeCacheKey(string proposalText, AnalyzeProposalRequest request)
    {
        var normalizedText = (proposalText ?? string.Empty).Trim();
        var projectValue = request.ProjectValue.HasValue
            ? request.ProjectValue.Value.ToString("F2", CultureInfo.InvariantCulture)
            : string.Empty;
        var hourlyRate = request.HourlyRate.HasValue
            ? request.HourlyRate.Value.ToString("F2", CultureInfo.InvariantCulture)
            : string.Empty;
        var additionalContext = (request.AdditionalContext ?? string.Empty).Trim();

        var payload = $"v1|{normalizedText}|{projectValue}|{hourlyRate}|{additionalContext}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<ProposalAnalysisResult?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(cacheKey))
        {
            return null;
        }

        try
        {
            await EnsureInitializedAsync(cancellationToken);

            await using var connection = new SqliteConnection(_options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT ResultJson FROM AnalysisCache WHERE CacheKey = @key LIMIT 1;";
            command.Parameters.AddWithValue("@key", cacheKey);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result is string json && !string.IsNullOrWhiteSpace(json))
            {
                _logger.LogInformation("Analysis cache hit for key '{CacheKey}'.", cacheKey);
                return JsonSerializer.Deserialize<ProposalAnalysisResult>(json);
            }

            _logger.LogInformation("Analysis cache miss for key '{CacheKey}'.", cacheKey);
            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to read from SQLite analysis cache for key '{CacheKey}'. Falling back to live analysis.", cacheKey);
            return null;
        }
    }

    public async Task SetAsync(string cacheKey, ProposalAnalysisResult result, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(cacheKey) || result == null)
        {
            return;
        }

        try
        {
            await EnsureInitializedAsync(cancellationToken);

            var json = JsonSerializer.Serialize(result);
            var createdAt = DateTime.UtcNow.ToString("O");

            await using var connection = new SqliteConnection(_options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO AnalysisCache (CacheKey, ResultJson, CreatedAtUtc)
                VALUES (@key, @json, @createdAt)
                ON CONFLICT(CacheKey) DO UPDATE SET
                    ResultJson = excluded.ResultJson,
                    CreatedAtUtc = excluded.CreatedAtUtc;
                """;
            command.Parameters.AddWithValue("@key", cacheKey);
            command.Parameters.AddWithValue("@json", json);
            command.Parameters.AddWithValue("@createdAt", createdAt);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Saved analysis result to SQLite cache for key '{CacheKey}'.", cacheKey);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to save result to SQLite analysis cache for key '{CacheKey}'.", cacheKey);
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
            {
                return;
            }

            await using var connection = new SqliteConnection(_options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                PRAGMA journal_mode = WAL;
                CREATE TABLE IF NOT EXISTS AnalysisCache (
                    CacheKey TEXT PRIMARY KEY,
                    ResultJson TEXT NOT NULL,
                    CreatedAtUtc TEXT NOT NULL
                );
                """;
            await command.ExecuteNonQueryAsync(cancellationToken);

            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
