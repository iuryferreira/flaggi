namespace Flaggi.Stores;

/// <summary>
/// In-memory implementation of <see cref="IFeatureStore"/>. Good for tests and seeding.
/// </summary>
/// 
public sealed class InMemoryFeatureStore(IEnumerable<Feature> features) : IFeatureStore
{
    private readonly Dictionary<string, Feature> _dict = features?.ToDictionary(f => f.Key, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, Feature>(StringComparer.OrdinalIgnoreCase);

    public Task<(IReadOnlyList<Feature> Features, string ETag)> GetAllAsync(CancellationToken ct = default)
    {
        var list = _dict.Values.ToList();
        var etag = Guid.NewGuid().ToString("N");
        return Task.FromResult<(IReadOnlyList<Feature>, string)>((list, etag));
    }

    public Task<Feature?> GetAsync(string key, CancellationToken ct = default)
    {
        _dict.TryGetValue(key, out var f);
        return Task.FromResult(f);
    }
}
