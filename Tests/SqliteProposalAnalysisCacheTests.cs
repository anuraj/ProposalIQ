using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProposalIQ.Web.Configuration;
using ProposalIQ.Web.Models;
using ProposalIQ.Web.Services;

namespace ProposalIQ.Web.Tests;

public class SqliteProposalAnalysisCacheTests
{
    [Fact]
    public void ComputeCacheKey_IsDeterministic_ForIdenticalInputs()
    {
        var cache = CreateCache("Data Source=:memory:");
        var request1 = new AnalyzeProposalRequest
        {
            ProjectValue = 10000m,
            HourlyRate = 120m,
            AdditionalContext = "Urgent project"
        };
        var request2 = new AnalyzeProposalRequest
        {
            ProjectValue = 10000m,
            HourlyRate = 120m,
            AdditionalContext = "Urgent project"
        };

        var key1 = cache.ComputeCacheKey("Sample proposal text", request1);
        var key2 = cache.ComputeCacheKey("Sample proposal text", request2);

        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ComputeCacheKey_ProducesDifferentKeys_WhenContextChanges()
    {
        var cache = CreateCache("Data Source=:memory:");
        var request1 = new AnalyzeProposalRequest { ProjectValue = 10000m };
        var request2 = new AnalyzeProposalRequest { ProjectValue = 20000m };

        var key1 = cache.ComputeCacheKey("Sample proposal text", request1);
        var key2 = cache.ComputeCacheKey("Sample proposal text", request2);

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyNotFound()
    {
        var dbPath = $"Data Source={Guid.NewGuid():N}.db";
        var cache = CreateCache(dbPath);

        try
        {
            var result = await cache.GetAsync("non-existent-key");
            Assert.Null(result);
        }
        finally
        {
            TryCleanupDb(dbPath);
        }
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_PersistsAndRetrievesResult()
    {
        var dbPath = $"Data Source={Guid.NewGuid():N}.db";
        var cache = CreateCache(dbPath);
        var expected = new ProposalAnalysisResult
        {
            OverallRisk = "High",
            ExecutiveSummary = "Significant scope risk detected.",
            Issues =
            [
                new ProposalIssue
                {
                    Category = "Scope",
                    Severity = "High",
                    Title = "Unlimited revisions clause",
                    Recommendation = "Limit to 2 revision rounds"
                }
            ]
        };

        try
        {
            var key = "test-key-123";
            await cache.SetAsync(key, expected);

            var retrieved = await cache.GetAsync(key);

            Assert.NotNull(retrieved);
            Assert.Equal("High", retrieved.OverallRisk);
            Assert.Equal("Significant scope risk detected.", retrieved.ExecutiveSummary);
            Assert.Single(retrieved.Issues);
            Assert.Equal("Unlimited revisions clause", retrieved.Issues[0].Title);
        }
        finally
        {
            TryCleanupDb(dbPath);
        }
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenCacheIsDisabled()
    {
        var options = Options.Create(new AnalysisCacheOptions
        {
            Enabled = false,
            ConnectionString = "Data Source=:memory:"
        });
        var cache = new SqliteProposalAnalysisCache(options, NullLogger<SqliteProposalAnalysisCache>.Instance);

        var key = "test-key";
        await cache.SetAsync(key, new ProposalAnalysisResult { OverallRisk = "Low" });
        var retrieved = await cache.GetAsync(key);

        Assert.Null(retrieved);
    }

    private static SqliteProposalAnalysisCache CreateCache(string connectionString)
    {
        var options = Options.Create(new AnalysisCacheOptions
        {
            Enabled = true,
            ConnectionString = connectionString
        });
        return new SqliteProposalAnalysisCache(options, NullLogger<SqliteProposalAnalysisCache>.Instance);
    }

    private static void TryCleanupDb(string connectionString)
    {
        try
        {
            var filename = connectionString.Replace("Data Source=", "").Trim();
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
        catch
        {
            // Ignore cleanup failure in test
        }
    }
}
