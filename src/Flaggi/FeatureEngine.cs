using System.Collections.Concurrent;

namespace Flaggi;

/// <summary>
/// Represents the default feature evaluation engine that determines whether a feature
/// is enabled or disabled for a given <see cref="FeatureContext"/>.
/// <para>
/// The engine delegates rule checks to registered <see cref="IRuleEvaluator"/> instances,
/// allowing extensibility through custom rule types.
/// </para>
/// </summary>
public sealed class FeatureEngine : IFeatureEvaluator
{
    private readonly ConcurrentDictionary<string, IRuleEvaluator> _evaluators;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureEngine"/> class.
    /// </summary>
    /// <param name="evaluators">
    /// An optional collection of <see cref="IRuleEvaluator"/> implementations to pre-register
    /// with this engine. Each evaluator is identified by its <see cref="IRuleEvaluator.Name"/>.
    /// </param>
    public FeatureEngine(IEnumerable<IRuleEvaluator>? evaluators = null)
    {
        _evaluators = new ConcurrentDictionary<string, IRuleEvaluator>(
            StringComparer.OrdinalIgnoreCase);

        if (evaluators != null)
        {
            foreach (var e in evaluators)
                _evaluators[e.Name] = e;
        }
    }

    /// <summary>
    /// Evaluates whether a given <see cref="Feature"/> is enabled in the provided
    /// <see cref="FeatureContext"/>.
    /// </summary>
    /// <param name="feature">The feature definition containing rules to evaluate.</param>
    /// <param name="ctx">The context describing the current user or environment.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>
    /// An <see cref="EvaluationResult"/> indicating whether the feature is enabled.
    /// </returns>
    public Task<EvaluationResult> EvaluateAsync(Feature feature, FeatureContext ctx, CancellationToken ct = default)
    {
        bool enabled = feature.IsEnabled;

        // Evaluate feature rules if the feature is initially enabled
        if (enabled && feature.Rules is { Count: > 0 })
        {
            foreach (var rule in feature.Rules)
            {
                if (_evaluators.TryGetValue(rule.RuleName, out var evaluator))
                {
                    enabled = enabled && evaluator.Evaluate(rule, ctx, feature.Key);
                }

                if (!enabled) break;
            }
        }

        // Disable feature if expired
        if (feature.ExpiresAt is not null && DateTimeOffset.UtcNow > feature.ExpiresAt)
        {
            enabled = false;
        }

        return Task.FromResult(new EvaluationResult(feature.Key, enabled));
    }

    /// <summary>
    /// Registers or replaces an <see cref="IRuleEvaluator"/> for use in rule evaluation.
    /// </summary>
    /// <param name="evaluator">The evaluator to register.</param>
    public void RegisterEvaluator(IRuleEvaluator evaluator)
        => _evaluators[evaluator.Name] = evaluator;
}
