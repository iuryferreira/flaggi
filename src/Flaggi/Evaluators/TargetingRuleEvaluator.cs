namespace Flaggi.Evaluators;

/// <summary>
/// Rule evaluator that enables a feature for specific users or groups.
/// <para>
/// The rule can contain:
/// <list type="bullet">
///   <item><description><c>users</c>: An array of user IDs.</description></item>
///   <item><description><c>groups</c>: An array of group names.</description></item>
///   <item><description><c>defaultRolloutPercentage</c>: (optional) fallback percentage rollout if no match occurs.</description></item>
/// </list>
/// </para>
/// </summary>
public sealed class TargetingRuleEvaluator : IRuleEvaluator
{
    /// <inheritdoc/>
    public string Name => "Targeting";

    /// <summary>
    /// Evaluates whether the feature should be enabled for the current context
    /// based on user ID, group membership, or optional fallback percentage rollout.
    /// </summary>
    /// <param name="rule">The feature rule with targeting parameters.</param>
    /// <param name="ctx">The current evaluation context.</param>
    /// <param name="featureKey">The key of the feature being evaluated.</param>
    /// <returns>
    /// <c>true</c> if the user or group matches, or if the fallback percentage is satisfied; otherwise, <c>false</c>.
    /// </returns>
    public bool Evaluate(FeatureRule rule, FeatureContext ctx, string featureKey)
    {
        // Check explicit users
        if (rule.Parameters.TryGetValue("users", out var u) && u is IEnumerable<object> userList)
        {
            var users = userList.Select(x => x.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (ctx.UserId is not null && users.Contains(ctx.UserId))
                return true;
        }

        // Check groups
        if (rule.Parameters.TryGetValue("groups", out var g) && g is IEnumerable<object> groupList)
        {
            var groups = groupList.Select(x => x.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (ctx.Groups.Any(grp => groups.Contains(grp)))
                return true;
        }

        // Optionally fallback to percentage rollout
        if (rule.Parameters.TryGetValue("defaultRolloutPercentage", out var p) && p is not null)
        {
            return new PercentageRuleEvaluator().Evaluate(
                new FeatureRule("Percentage", new Dictionary<string, object?> { ["value"] = p }),
                ctx, featureKey);
        }

        return false;
    }
}
