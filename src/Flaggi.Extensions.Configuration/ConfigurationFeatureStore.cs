using Microsoft.Extensions.Configuration;

namespace Flaggi.Extensions.Configuration;

/// <summary>
/// Reads features from IConfiguration section "Features". Works with appsettings and Azure App Configuration.
/// </summary>
public sealed class ConfigurationFeatureStore : IFeatureStore
{
    private readonly IConfiguration _configuration;

    public ConfigurationFeatureStore(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public Task<(IReadOnlyList<Feature> Features, string ETag)> GetAllAsync(CancellationToken ct = default)
    {
        var section = _configuration.GetSection("Features");
        var features = section.Get<List<Feature>>() ?? [];
        var etag = DateTimeOffset.UtcNow.Ticks.ToString();
        return Task.FromResult<(IReadOnlyList<Feature>, string)>((features, etag));
    }

    public async Task<Feature?> GetAsync(string key, CancellationToken ct = default)
    {
        var (features, _) = await GetAllAsync(ct);
        return features.FirstOrDefault(f => f.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }
}

