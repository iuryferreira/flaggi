namespace Flaggi;


/// <summary>
/// Represents a feature with a unique key, its enabled state, optional rules, and an optional expiration date.
/// </summary>
/// <param name="Key">The unique identifier for the feature.</param>
/// <param name="IsEnabled">Indicates whether the feature is enabled.</param>
/// <param name="Rules">An optional list of rules associated with the feature.</param>
/// <param name="ExpiresAt">An optional expiration date and time for the feature.</param>
public sealed record Feature(string Key, bool IsEnabled, IReadOnlyList<FeatureRule>? Rules = null, DateTimeOffset? ExpiresAt = null);


/// <summary>
/// Represents the context of a feature, containing information about the user, 
/// their associated groups, and additional properties.
/// </summary>
/// <param name="UserId">The identifier of the user. This can be <c>null</c> if the user is not specified. </param>
/// <param name="Groups"> A read-only list of groups associated with the user.</param>
/// <param name="Properties"> A dictionary containing additional properties related to the feature context. 
/// The keys are strings, and the values can be any object or <c>null</c>.
/// </param>

public sealed record FeatureContext(string? UserId, IReadOnlyList<string> Groups, IDictionary<string, object?> Properties);

