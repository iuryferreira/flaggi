using System.Security.Cryptography;
using System.Text;

namespace Flaggi.Evaluators;

/// <summary>
/// Rule evaluator that enables a feature for a percentage of users.
/// <para>
/// The percentage rollout is deterministic and based on the combination of
/// <c>featureKey</c> and <c>UserId</c> from the <see cref="FeatureContext"/>.
/// </para>
/// </summary>
public sealed class PercentageRuleEvaluator : IRuleEvaluator
{
    /// <inheritdoc/>
    public string Name => "Percentage";

    /// <summary>
    /// Evaluates whether the feature should be enabled for the current context
    /// based on a rollout percentage.
    /// </summary>
    /// <param name="rule">
    /// The feature rule containing a <c>value</c> parameter indicating the rollout percentage (0–100).
    /// </param>
    /// <param name="ctx">The current evaluation context.</param>
    /// <param name="featureKey">The key of the feature being evaluated.</param>
    /// <returns>
    /// <c>true</c> if the computed hash value is within the rollout percentage; otherwise, <c>false</c>.
    /// </returns>
    public bool Evaluate(FeatureRule rule, FeatureContext ctx, string featureKey)
    {
        if (!rule.Parameters.TryGetValue("value", out var raw) || raw is null)
            return false;

        int percentage = Convert.ToInt32(raw);

        // Use UserId as seed to ensure consistent rollout
        var discriminator = ctx.UserId ?? "anonymous";
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{featureKey}:{discriminator}"));
        var value = BitConverter.ToUInt32(bytes, 0) % 100;

        return value < percentage;
    }
}
