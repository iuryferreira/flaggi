using Flaggi.Evaluators;

namespace Flaggi.Test.Evaluators;

public class TargetingRuleEvaluatorTest
{
    [Fact(DisplayName = "TargetingRuleEvaluator - Evaluate should return false when no parameters provided")]
    public void Evaluate_ShouldReturnFalse_WhenNoParametersProvided()
    {
        // Arrange
        var evaluator = new TargetingRuleEvaluator();
        var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?>());
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        // Act
        var result = evaluator.Evaluate(rule, ctx, "Feature.A");

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "TargetingRuleEvaluator - Evaluate should return false when user does not match")]
    public void Evaluate_ShouldReturnFalse_WhenUserNotInList()
    {
        // Arrange
        var evaluator = new TargetingRuleEvaluator();
        var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?>
        {
            ["users"] = new object[] { "otherUser" }
        });
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        // Act
        var result = evaluator.Evaluate(rule, ctx, "Feature.A");

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "TargetingRuleEvaluator - Evaluate should return false when groups do not match")]
    public void Evaluate_ShouldReturnFalse_WhenGroupsDoNotMatch()
    {
        // Arrange
        var evaluator = new TargetingRuleEvaluator();
        var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?>
        {
            ["groups"] = new object[] { "Admins", "Managers" }
        });
        var ctx = new FeatureContext("user1", ["Users"], new Dictionary<string, object?>());

        // Act
        var result = evaluator.Evaluate(rule, ctx, "Feature.A");

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "TargetingRuleEvaluator - Evaluate should return false when fallback percentage is zero")]
    public void Evaluate_ShouldReturnFalse_WhenFallbackIsZero()
    {
        // Arrange
        var evaluator = new TargetingRuleEvaluator();
        var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?>
        {
            ["defaultRolloutPercentage"] = 0
        });
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        // Act
        var result = evaluator.Evaluate(rule, ctx, "Feature.A");

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "TargetingRuleEvaluator - Evaluate should return true when user matches")]
    public void Evaluate_ShouldReturnTrue_WhenUserInList()
    {
        // Arrange
        var evaluator = new TargetingRuleEvaluator();
        var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?>
        {
            ["users"] = new object[] { "user1", "user2" }
        });
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        // Act
        var result = evaluator.Evaluate(rule, ctx, "Feature.A");

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "TargetingRuleEvaluator - Evaluate should return true when group matches")]
    public void Evaluate_ShouldReturnTrue_WhenGroupMatches()
    {
        // Arrange
        var evaluator = new TargetingRuleEvaluator();
        var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?>
        {
            ["groups"] = new object[] { "BetaTesters", "QA" }
        });
        var ctx = new FeatureContext("userX", ["Readers", "BetaTesters"], new Dictionary<string, object?>());

        // Act
        var result = evaluator.Evaluate(rule, ctx, "Feature.A");

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "TargetingRuleEvaluator - Evaluate should respect defaultRolloutPercentage when no user or group matches")]
    public void Evaluate_ShouldRespectFallback_WhenNoUserOrGroupMatches()
    {
        // Arrange
        var evaluator = new TargetingRuleEvaluator();
        var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?>
        {
            ["defaultRolloutPercentage"] = 100
        });
        var ctx = new FeatureContext("userX", ["Readers"], new Dictionary<string, object?>());

        // Act
        var result = evaluator.Evaluate(rule, ctx, "Feature.A");

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "TargetingRuleEvaluator - Evaluate should be case-insensitive for users and groups")]
    public void Evaluate_ShouldBeCaseInsensitive_ForUsersAndGroups()
    {
        // Arrange
        var evaluator = new TargetingRuleEvaluator();
        var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?>
        {
            ["users"] = new object[] { "USER1" },
            ["groups"] = new object[] { "BETATESTERS" }
        });
        var ctx = new FeatureContext("user1", ["betatesters"], new Dictionary<string, object?>());

        // Act
        var result = evaluator.Evaluate(rule, ctx, "Feature.A");

        // Assert
        Assert.True(result);
    }
}
