namespace Flaggi;

/// <summary>
/// Represents a persistence abstraction for managing and retrieving <see cref="Feature"/> definitions.
/// <para>
/// A feature store can be backed by any source, such as in-memory collections, configuration files,
/// databases, or external services (e.g., Azure App Configuration).
/// </para>
/// </summary>
public interface IFeatureStore
{
    /// <summary>
    /// Retrieves all features from the store along with an associated ETag.
    /// <para>
    /// The ETag can be used for caching and concurrency control. Clients may use it to determine
    /// whether the feature set has changed since the last fetch.
    /// </para>
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is a tuple containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Features</c>: A read-only list of <see cref="Feature"/> instances.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>ETag</c>: A string representing the version of the feature set.</description>
    ///   </item>
    /// </list>
    /// </returns>
    Task<(IReadOnlyList<Feature> Features, string ETag)> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a specific feature definition by its unique key.
    /// </summary>
    /// <param name="key">The unique identifier of the feature.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the <see cref="Feature"/>
    /// if found; otherwise, <c>null</c>.
    /// </returns>
    Task<Feature?> GetAsync(string key, CancellationToken ct = default);
}
