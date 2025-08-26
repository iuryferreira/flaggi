using System.Text.Json;

namespace Flaggi.Stores;

/// <summary>
/// File-based implementation of <see cref="IFeatureStore"/>. Reads from a JSON file like "flaggi.json or appsettings.json specified".
/// </summary>
public sealed class JsonFileFeatureStore : IFeatureStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public JsonFileFeatureStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must be provided.", nameof(filePath));
        }
        _filePath = filePath;
    }

    public async Task<(IReadOnlyList<Feature> Features, string ETag)> GetAllAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath))
        {
            return (Array.Empty<Feature>(), "missing-file");
        }
        await using var stream = File.OpenRead(_filePath);
        var doc = await JsonSerializer.DeserializeAsync<JsonFeatureDocument>(stream, _options, ct);
        var features = doc?.Features ?? [];
        var etag = File.GetLastWriteTimeUtc(_filePath).Ticks.ToString();
        return (features, etag);
    }

    public async Task<Feature?> GetAsync(string key, CancellationToken ct = default)
    {
        var (all, _) = await GetAllAsync(ct);
        return all.FirstOrDefault(f => f.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class JsonFeatureDocument
    {
        public List<Feature> Features { get; set; } = [];
    }
}

