namespace Flaggi;


/// <summary>
/// Defines a contract for evaluating a feature based on the provided context and returning the evaluation result.
/// </summary>
public interface IFeatureEvaluator
{
    /// <summary>
    /// Asynchronously evaluates a feature based on the provided feature context and cancellation token.
    /// </summary>
    /// <param name="feature">The feature to be evaluated.</param>
    /// <param name="ctx">The context containing information required for the evaluation.</param>
    /// <param name="ct">An optional cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the evaluation result.</returns>
    Task<EvaluationResult> EvaluateAsync(Feature feature, FeatureContext ctx, CancellationToken ct = default);
}


/// <summary>
/// Represents the result of a feature evaluation, indicating whether a specific feature is enabled or not.
/// </summary>
/// <param name="FeatureKey">The unique key identifying the feature being evaluated.</param>
/// <param name="Enabled">A boolean value indicating whether the feature is enabled (<c>true</c>) or disabled (<c>false</c>).</param>
public sealed record EvaluationResult(string FeatureKey, bool Enabled);