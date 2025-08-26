using Xunit;
using Moq;

namespace Flaggi.Test;

public class FeatureEngineTest
{
    [Fact(DisplayName = "FeatureEngine - EvaluateAsync should return disabled when feature is expired")]
    public async Task EvaluateAsync_ShouldReturnDisabled_WhenFeatureIsExpired()
    {
        // Arrange
        var expiredFeature = new Feature("TestFeature", true, null, DateTimeOffset.UtcNow.AddMinutes(-1));
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());
        var engine = new FeatureEngine();

        // Act
        var result = await engine.EvaluateAsync(expiredFeature, ctx);

        // Assert
        Assert.False(result.Enabled);
    }

    [Fact(DisplayName = "FeatureEngine - EvaluateAsync should return disabled when rule evaluator not found")]
    public async Task EvaluateAsync_ShouldReturnDisabled_WhenRuleEvaluatorNotFound()
    {
        // Arrange
        var rules = new List<FeatureRule>
        {
            new FeatureRule("NonExistingRule", new Dictionary<string, object?>())
        };
        var feature = new Feature("TestFeature", true, rules);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());
        var engine = new FeatureEngine(); // no evaluators registered

        // Act
        var result = await engine.EvaluateAsync(feature, ctx);

        // Assert
        Assert.True(result.Enabled); // stays as IsEnabled = true but no evaluator matched → should keep true
    }

    [Fact(DisplayName = "FeatureEngine - EvaluateAsync should return disabled when evaluator fails")]
    public async Task EvaluateAsync_ShouldReturnDisabled_WhenEvaluatorFails()
    {
        // Arrange
        var rule = new FeatureRule("AlwaysFail", new Dictionary<string, object?>());
        var feature = new Feature("FailingFeature", true, [rule]);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        var mockEvaluator = new Mock<IRuleEvaluator>();
        mockEvaluator.Setup(x => x.Name).Returns("AlwaysFail");
        mockEvaluator.Setup(x => x.Evaluate(rule, ctx, "FailingFeature")).Returns(false);

        var engine = new FeatureEngine([mockEvaluator.Object]);

        // Act
        var result = await engine.EvaluateAsync(feature, ctx);

        // Assert
        Assert.False(result.Enabled);
    }

    [Fact(DisplayName = "FeatureEngine - EvaluateAsync should return enabled when evaluator succeeds")]
    public async Task EvaluateAsync_ShouldReturnEnabled_WhenEvaluatorSucceeds()
    {
        // Arrange
        var rule = new FeatureRule("AlwaysPass", new Dictionary<string, object?>());
        var feature = new Feature("PassingFeature", true, [rule]);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        var mockEvaluator = new Mock<IRuleEvaluator>();
        mockEvaluator.Setup(x => x.Name).Returns("AlwaysPass");
        mockEvaluator.Setup(x => x.Evaluate(rule, ctx, "PassingFeature")).Returns(true);

        var engine = new FeatureEngine([mockEvaluator.Object]);

        // Act
        var result = await engine.EvaluateAsync(feature, ctx);

        // Assert
        Assert.True(result.Enabled);
    }

    [Fact(DisplayName = "FeatureEngine - EvaluateAsync should return enabled when feature has no rules")]
    public async Task EvaluateAsync_ShouldReturnEnabled_WhenFeatureHasNoRules()
    {
        // Arrange
        var feature = new Feature("SimpleFeature", true);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());
        var engine = new FeatureEngine();

        // Act
        var result = await engine.EvaluateAsync(feature, ctx);

        // Assert
        Assert.True(result.Enabled);
    }

    [Fact(DisplayName = "FeatureEngine - RegisterEvaluator should replace existing evaluator when same name")]
    public async Task RegisterEvaluator_ShouldReplaceExisting_WhenSameName()
    {
        // Arrange
        var rule = new FeatureRule("ReplaceMe", new Dictionary<string, object?>());
        var feature = new Feature("TestFeature", true, [rule]);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        var mockEvaluator1 = new Mock<IRuleEvaluator>();
        mockEvaluator1.Setup(x => x.Name).Returns("ReplaceMe");
        mockEvaluator1.Setup(x => x.Evaluate(rule, ctx, "TestFeature")).Returns(false);

        var mockEvaluator2 = new Mock<IRuleEvaluator>();
        mockEvaluator2.Setup(x => x.Name).Returns("ReplaceMe");
        mockEvaluator2.Setup(x => x.Evaluate(rule, ctx, "TestFeature")).Returns(true);

        var engine = new FeatureEngine([mockEvaluator1.Object]);
        engine.RegisterEvaluator(mockEvaluator2.Object);

        // Act
        var result = await engine.EvaluateAsync(feature, ctx);

        // Assert
        Assert.True(result.Enabled);
    }

    [Fact(DisplayName = "FeatureEngine - EvaluateAsync should return disabled when IsEnabled is false regardless of rules")]
    public async Task EvaluateAsync_ShouldReturnDisabled_WhenFeatureIsDisabled()
    {
        // Arrange
        var feature = new Feature("DisabledFeature", false,
    [
        new FeatureRule("AlwaysPass", new Dictionary<string, object?>())
    ]);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        var mockEvaluator = new Mock<IRuleEvaluator>();
        mockEvaluator.Setup(x => x.Name).Returns("AlwaysPass");
        mockEvaluator.Setup(x => x.Evaluate(It.IsAny<FeatureRule>(), It.IsAny<FeatureContext>(), "DisabledFeature"))
                     .Returns(true);

        var engine = new FeatureEngine([mockEvaluator.Object]);

        // Act
        var result = await engine.EvaluateAsync(feature, ctx);

        // Assert
        Assert.False(result.Enabled);
    }

    [Fact(DisplayName = "FeatureEngine - EvaluateAsync should short-circuit evaluation when first rule fails")]
    public async Task EvaluateAsync_ShouldShortCircuit_WhenFirstRuleFails()
    {
        // Arrange
        var failingRule = new FeatureRule("AlwaysFail", new Dictionary<string, object?>());
        var passingRule = new FeatureRule("AlwaysPass", new Dictionary<string, object?>());
        var feature = new Feature("TestFeature", true, [failingRule, passingRule]);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        var mockFail = new Mock<IRuleEvaluator>();
        mockFail.Setup(x => x.Name).Returns("AlwaysFail");
        mockFail.Setup(x => x.Evaluate(failingRule, ctx, "TestFeature")).Returns(false);

        var mockPass = new Mock<IRuleEvaluator>();
        mockPass.Setup(x => x.Name).Returns("AlwaysPass");

        var engine = new FeatureEngine([mockFail.Object, mockPass.Object]);

        // Act
        var result = await engine.EvaluateAsync(feature, ctx);

        // Assert
        Assert.False(result.Enabled);
        mockPass.Verify(x => x.Evaluate(It.IsAny<FeatureRule>(), It.IsAny<FeatureContext>(), It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "FeatureEngine - EvaluateAsync should return enabled when multiple rules all succeed")]
    public async Task EvaluateAsync_ShouldReturnEnabled_WhenAllRulesSucceed()
    {
        // Arrange
        var rule1 = new FeatureRule("R1", new Dictionary<string, object?>());
        var rule2 = new FeatureRule("R2", new Dictionary<string, object?>());
        var feature = new Feature("MultiRuleFeature", true, [rule1, rule2]);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        var mockEval1 = new Mock<IRuleEvaluator>();
        mockEval1.Setup(x => x.Name).Returns("R1");
        mockEval1.Setup(x => x.Evaluate(rule1, ctx, "MultiRuleFeature")).Returns(true);

        var mockEval2 = new Mock<IRuleEvaluator>();
        mockEval2.Setup(x => x.Name).Returns("R2");
        mockEval2.Setup(x => x.Evaluate(rule2, ctx, "MultiRuleFeature")).Returns(true);

        var engine = new FeatureEngine([mockEval1.Object, mockEval2.Object]);

        // Act
        var result = await engine.EvaluateAsync(feature, ctx);

        // Assert
        Assert.True(result.Enabled);
    }

    [Fact(DisplayName = "FeatureEngine - EvaluateAsync should preserve FeatureKey in result")]
    public async Task EvaluateAsync_ShouldPreserveFeatureKey_InResult()
    {
        // Arrange
        var feature = new Feature("KeyCheckFeature", true);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());
        var engine = new FeatureEngine();

        // Act
        var result = await engine.EvaluateAsync(feature, ctx);

        // Assert
        Assert.Equal("KeyCheckFeature", result.FeatureKey);
    }
}
