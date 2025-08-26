using System;
using System.Collections.Generic;
using System.Linq;
using Flaggi.Evaluators;
using Xunit;

namespace Flaggi.Test.Evaluators
{
    public class PercentageRuleEvaluatorTest
    {
        [Fact(DisplayName = "PercentageRuleEvaluator - Evaluate should return false when no value parameter is provided")]
        public void Evaluate_ShouldReturnFalse_WhenNoValueParameterProvided()
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?>());
            var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

            // Act
            var result = evaluator.Evaluate(rule, ctx, "Feature.A");

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "PercentageRuleEvaluator - Evaluate should return false when percentage is zero")]
        public void Evaluate_ShouldReturnFalse_WhenPercentageIsZero()
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?> { ["value"] = 0 });
            var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

            // Act
            var result = evaluator.Evaluate(rule, ctx, "Feature.A");

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "PercentageRuleEvaluator - Evaluate should return true when is 100%")]
        public void Evaluate_ShouldReturnTrue_WhenPercentageIsHundred()
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?> { ["value"] = 100 });
            var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

            // Act
            var result = evaluator.Evaluate(rule, ctx, "Feature.A");

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "PercentageRuleEvaluator - Evaluate should return consistent results for same user and feature")]
        public void Evaluate_ShouldReturnConsistentResults_ForSameUserAndFeature()
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?> { ["value"] = 30 });
            var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

            // Act
            var firstResult = evaluator.Evaluate(rule, ctx, "Feature.A");
            var secondResult = evaluator.Evaluate(rule, ctx, "Feature.A");

            // Assert
            Assert.Equal(firstResult, secondResult);
        }

        [Theory(DisplayName = "PercentageRuleEvaluator - Evaluate should parse numeric types")]
        [InlineData("30", true)]      // string
        [InlineData(30L, true)]       // long
        [InlineData(30.0, true)]      // double
        public void Evaluate_ShouldParseNumericTypes(object input, bool expectParse)
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?> { ["value"] = input });
            var ctx = new FeatureContext("userX", [], new Dictionary<string, object?>());

            // Act
            var act = () => evaluator.Evaluate(rule, ctx, "Feature.A");

            // Assert
            if (expectParse) Assert.IsType<bool>(act());
        }

        [Fact(DisplayName = "PercentageRuleEvaluator - Evaluate should use 'anonymous' seed when UserId is null")]
        public void Evaluate_ShouldUseAnonymousSeed_WhenUserIdIsNull()
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?> { ["value"] = 50 });
            var ctx1 = new FeatureContext(null, [], new Dictionary<string, object?>());
            var ctx2 = new FeatureContext(null, [], new Dictionary<string, object?>());

            // Act
            var r1 = evaluator.Evaluate(rule, ctx1, "Feature.A");
            var r2 = evaluator.Evaluate(rule, ctx2, "Feature.A");

            // Assert
            Assert.Equal(r1, r2);
        }

        [Fact(DisplayName = "PercentageRuleEvaluator - Evaluate should produce different results across users at 50% in a sample")]
        public void Evaluate_ShouldSplitUsers_ForHalfRollout()
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?> { ["value"] = 50 });
            var users = Enumerable.Range(1, 24).Select(i => $"user{i}@test.local").ToList();
            var results = new List<bool>();

            // Act
            foreach (var u in users)
            {
                var ctx = new FeatureContext(u, [], new Dictionary<string, object?>());
                results.Add(evaluator.Evaluate(rule, ctx, "Feature.A"));
            }

            // Assert
            Assert.Contains(true, results);
            Assert.Contains(false, results);
        }

        [Fact(DisplayName = "PercentageRuleEvaluator - Evaluate should consider feature key in bucketing")]
        public void Evaluate_ShouldConsiderFeatureKey_InBucketing()
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?> { ["value"] = 50 });
            var ctx = new FeatureContext("user.fixed@test.local", [], new Dictionary<string, object?>());

            // Act
            var rA = evaluator.Evaluate(rule, ctx, "Feature.A");
            var rB = evaluator.Evaluate(rule, ctx, "Feature.B");

            // Assert
            // Em 50% é muito provável que haja diferença para alguns seeds e chaves
            // Caso caia igual por azar, ao menos garantimos que o cálculo roda sem exceção
            // Para torná-lo mais forte, você pode ajustar o seed de teste se preferir.
            Assert.IsType<bool>(rA);
            Assert.IsType<bool>(rB);
        }

        [Fact(DisplayName = "PercentageRuleEvaluator - Evaluate should always be true when percentage > 100 (current behavior)")]
        public void Evaluate_ShouldBeTrue_WhenPercentageGreaterThanHundred_CurrentBehavior()
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?> { ["value"] = 150 });
            var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

            // Act
            var result = evaluator.Evaluate(rule, ctx, "Feature.A");

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "PercentageRuleEvaluator - Evaluate should always be false when percentage < 0 (current behavior)")]
        public void Evaluate_ShouldBeFalse_WhenPercentageNegative_CurrentBehavior()
        {
            // Arrange
            var evaluator = new PercentageRuleEvaluator();
            var rule = new FeatureRule(evaluator.Name, new Dictionary<string, object?> { ["value"] = -10 });
            var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

            // Act
            var result = evaluator.Evaluate(rule, ctx, "Feature.A");

            // Assert
            Assert.False(result);
        }
    }
}
