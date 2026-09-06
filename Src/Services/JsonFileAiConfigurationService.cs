using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProposalIQ.Web.Configuration;

namespace ProposalIQ.Web.Services;

public class JsonFileAiConfigurationService : IAiConfigurationService
{
    private readonly string _filePath;
    private readonly ILogger<JsonFileAiConfigurationService> _logger;
    private readonly object _gate = new();
    private AiProviderOptions _currentOptions;

    public JsonFileAiConfigurationService(
        IOptions<AiProviderOptions> initialOptions,
        ILogger<JsonFileAiConfigurationService> logger,
        string? filePath = null)
    {
        _logger = logger;
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "ai-settings.json");
        _currentOptions = LoadOrCreate(initialOptions.Value);
    }

    public AiProviderOptions GetOptions()
    {
        lock (_gate)
        {
            return Clone(_currentOptions);
        }
    }

    public async Task UpdateOptionsAsync(AiProviderOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var cloned = Clone(options);

        lock (_gate)
        {
            _currentOptions = cloned;
        }

        try
        {
            var json = JsonSerializer.Serialize(cloned, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
            _logger.LogInformation("Saved updated AI provider options to '{FilePath}'. Active provider: {Provider}", _filePath, cloned.Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist AI provider options to '{FilePath}'.", _filePath);
            throw;
        }
    }

    private AiProviderOptions LoadOrCreate(AiProviderOptions defaultOptions)
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var loaded = JsonSerializer.Deserialize<AiProviderOptions>(json);
                if (loaded != null)
                {
                    _logger.LogInformation("Loaded AI provider options from '{FilePath}'. Active provider: {Provider}", _filePath, loaded.Provider);
                    return loaded;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load AI provider options from '{FilePath}'. Falling back to default options.", _filePath);
        }

        return Clone(defaultOptions);
    }

    private static AiProviderOptions Clone(AiProviderOptions source)
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<AiProviderOptions>(json) ?? new AiProviderOptions();
    }
}
