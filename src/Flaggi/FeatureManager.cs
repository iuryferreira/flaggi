namespace Flaggi;

/// <summary>
/// Defines the high-level contract for checking whether features are enabled.
/// <para>
/// This abstraction hides the underlying <see cref="IFeatureStore"/> and <see cref="IFeatureEvaluator"/>
/// logic, exposing only methods for evaluating feature status.
/// </para>
/// </summary>
public interface IFeatureManager
{
    /// <summary>
    /// Checks whether a feature is enabled for the given context.
    /// </summary>
    /// <param name="featureKey">The unique key of the feature to evaluate.</param>
    /// <param name="ctx">The evaluation context (e.g., user, groups, custom properties).</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is <c>true</c>
    /// if the feature is enabled for the specified context; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsEnabledAsync(string featureKey, FeatureContext ctx, CancellationToken ct = default);

    /// <summary>
    /// Evaluates a feature and returns a detailed result, including the feature key and its enabled status.
    /// <para>
    /// This is useful when additional information beyond a simple boolean is required.
    /// </para>
    /// </summary>
    /// <param name="featureKey">The unique key of the feature to evaluate.</param>
    /// <param name="ctx">The evaluation context (e.g., user, groups, custom properties).</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is an <see cref="EvaluationResult"/>
    /// containing the feature key and whether the feature is enabled.
    /// </returns>
    Task<EvaluationResult> EvaluateAsync(string featureKey, FeatureContext ctx, CancellationToken ct = default);
}

/// <summary>
/// Provides a high-level API for checking whether features are enabled,
/// abstracting away the underlying evaluation and storage logic.
/// <para>
/// The <see cref="FeatureManager"/> coordinates between the configured
/// <see cref="IFeatureStore"/> (where features are defined) and the <see cref="IFeatureEvaluator"/>
/// (which applies rules and conditions).
/// </para>
/// </summary>
public sealed class FeatureManager(IFeatureStore store, IFeatureEvaluator engine) : IFeatureManager
{
    private readonly IFeatureStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly IFeatureEvaluator _engine = engine ?? throw new ArgumentNullException(nameof(engine));

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync(string featureKey, FeatureContext ctx, CancellationToken ct = default)
    {
        var feature = await _store.GetAsync(featureKey, ct);
        if (feature is null)
        {
            return false; // feature not registered => disabled
        }

        var result = await _engine.EvaluateAsync(feature, ctx, ct);
        return result.Enabled;
    }

    /// <inheritdoc/>
    public async Task<EvaluationResult> EvaluateAsync(string featureKey, FeatureContext ctx, CancellationToken ct = default)
    {
        var feature = await _store.GetAsync(featureKey, ct);
        if (feature is null)
        {
            return new EvaluationResult(featureKey, false);
        }

        return await _engine.EvaluateAsync(feature, ctx, ct);
    }
}
