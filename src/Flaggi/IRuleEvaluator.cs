namespace Flaggi;

/// <summary>
/// Contract for individual rule evaluators used by the <see cref="FeatureEngine"/>.
/// <para>
/// A rule evaluator encapsulates the logic for a specific rule type (e.g., "Percentage", "Targeting").
/// </para>
/// </summary>
public interface IRuleEvaluator
{
    /// <summary>
    /// Gets the unique name of the rule that this evaluator supports.
    /// <para>
    /// This name is matched against the <see cref="FeatureRule.RuleName"/> when evaluating rules.
    /// </para>
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Evaluates a given <see cref="FeatureRule"/> within a specified <see cref="FeatureContext"/>.
    /// </summary>
    /// <param name="rule">The feature rule containing the rule name and parameters.</param>
    /// <param name="ctx">The evaluation context that provides user and environment information.</param>
    /// <param name="featureKey">The key of the feature being evaluated, useful for consistent hashing.</param>
    /// <returns>
    /// <c>true</c> if the rule conditions are satisfied and the feature should remain enabled; 
    /// otherwise, <c>false</c>.
    /// </returns>
    bool Evaluate(FeatureRule rule, FeatureContext ctx, string featureKey);
}

/// <summary>
/// Represents a rule definition associated with a <see cref="Feature"/>.
/// <para>
/// Each rule specifies a <see cref="RuleName"/> that matches a registered <see cref="IRuleEvaluator"/> 
/// and a collection of <see cref="Parameters"/> used during evaluation.
/// </para>
/// </summary>
/// <param name="RuleName">
/// The unique name of the rule type (e.g., "Percentage", "Targeting", "Schedule").
/// </param>
/// <param name="Parameters">
/// A dictionary of parameters required for evaluating the rule (e.g., { "value": 30 }).
/// </param>
public sealed record FeatureRule(string RuleName, IDictionary<string, object?> Parameters);
