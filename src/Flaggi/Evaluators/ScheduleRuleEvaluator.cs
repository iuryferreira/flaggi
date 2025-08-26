namespace Flaggi.Evaluators;

/// <summary>
/// Rule evaluator that enables a feature only within a defined time window.
/// <para>
/// The rule may specify:
/// <list type="bullet">
///   <item><description><c>from</c>: The start date/time (ISO 8601 string).</description></item>
///   <item><description><c>to</c>: The end date/time (ISO 8601 string).</description></item>
/// </list>
/// If no values are provided, the feature is considered always enabled by this rule.
/// </para>
/// </summary>
public sealed class ScheduleRuleEvaluator : IRuleEvaluator
{
    /// <inheritdoc/>
    public string Name => "Schedule";

    /// <summary>
    /// Evaluates whether the current time falls within the configured schedule window.
    /// </summary>
    /// <param name="rule">The feature rule with optional <c>from</c> and <c>to</c> parameters.</param>
    /// <param name="ctx">The current evaluation context (not used in this evaluator).</param>
    /// <param name="featureKey">The key of the feature being evaluated.</param>
    /// <returns>
    /// <c>true</c> if the current UTC time is within the specified window; otherwise, <c>false</c>.
    /// </returns>
    public bool Evaluate(FeatureRule rule, FeatureContext ctx, string featureKey)
    {
        var from = rule.Parameters.TryGetValue("from", out var f)
            ? DateTimeOffset.Parse(f!.ToString()!)
            : DateTimeOffset.MinValue;

        var to = rule.Parameters.TryGetValue("to", out var t)
            ? DateTimeOffset.Parse(t!.ToString()!)
            : DateTimeOffset.MaxValue;

        var now = DateTimeOffset.UtcNow;
        return now >= from && now <= to;
    }
}
