namespace Flaggi.Stores
{
    /// <summary>
    /// An <see cref="IFeatureStore"/> that composes multiple stores into a single read view.
    /// <para>
    /// <b>Merging semantics for <see cref="GetAllAsync(CancellationToken)"/>:</b>
    /// All features from all inner stores are combined. When duplicate keys exist the
    /// last occurrence wins in the merged list. The returned ETag is a concatenation
    /// of inner store ETags separated by the pipe character.
    /// </para>
    /// <para>
    /// <b>Lookup semantics for <see cref="GetAsync(string, CancellationToken)"/>:</b>
    /// Stores are queried in the order they were provided to the constructor.
    /// The first non null match is returned. This means store order defines lookup priority.
    /// </para>
    /// </summary>
    public sealed class CompositeFeatureStore : IFeatureStore
    {
        private readonly IReadOnlyList<IFeatureStore> _stores;

        /// <summary>
        /// Creates a new composite store over the given sequence of stores.
        /// Store order matters because it defines lookup priority for <see cref="GetAsync(string, CancellationToken)"/>.
        /// </summary>
        /// <param name="stores">The sequence of stores to compose. The sequence must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stores"/> is null.</exception>
        public CompositeFeatureStore(IEnumerable<IFeatureStore> stores)
        {
            if (stores is null) throw new ArgumentNullException(nameof(stores));
            _stores = stores.ToList();
        }

        /// <summary>
        /// Retrieves all features by aggregating results from the inner stores.
        /// <para>
        /// When duplicate feature keys exist the last occurrence in the aggregated sequence wins.
        /// The returned ETag is the concatenation of inner ETags using the pipe character as a separator.
        /// </para>
        /// </summary>
        /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>
        /// A tuple with the merged list of features and a composite ETag string.
        /// </returns>
        public async Task<(IReadOnlyList<Feature> Features, string ETag)> GetAllAsync(CancellationToken ct = default)
        {
            var allFeatures = new List<Feature>();
            var etags = new List<string>();

            foreach (var store in _stores)
            {
                var (features, etag) = await store.GetAllAsync(ct).ConfigureAwait(false);
                allFeatures.AddRange(features);
                etags.Add(etag);
            }

            var merged = allFeatures
                .GroupBy(f => f.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Last())
                .ToList();

            return (merged, string.Join("|", etags));
        }

        /// <summary>
        /// Retrieves a feature by querying the inner stores in the order they were provided.
        /// The first non null result is returned which makes earlier stores have higher priority during lookups.
        /// </summary>
        /// <param name="key">The unique feature key to look up.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>
        /// The matching <see cref="Feature"/> if found or null when none of the inner stores contain the key.
        /// </returns>
        public async Task<Feature?> GetAsync(string key, CancellationToken ct = default)
        {
            foreach (var store in _stores)
            {
                var feature = await store.GetAsync(key, ct).ConfigureAwait(false);
                if (feature is not null)
                    return feature;
            }

            return null;
        }
    }
}
