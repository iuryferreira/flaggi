using Moq;

namespace Flaggi.Test;

public class FeatureManagerTest
{
    [Fact(DisplayName = "FeatureManager - Ctor should throw when store is null")]
    public void Ctor_ShouldThrow_WhenStoreIsNull()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => new FeatureManager(null!, Mock.Of<IFeatureEvaluator>()));
    }

    [Fact(DisplayName = "FeatureManager - Ctor should throw when engine is null")]
    public void Ctor_ShouldThrow_WhenEngineIsNull()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => new FeatureManager(Mock.Of<IFeatureStore>(), null!));
    }

    [Fact(DisplayName = "FeatureManager - IsEnabledAsync should return false when feature not found")]
    public async Task IsEnabledAsync_ShouldReturnFalse_WhenFeatureNotFound()
    {
        // Arrange
        var mockStore = new Mock<IFeatureStore>();
        mockStore.Setup(s => s.GetAsync("MissingFeature", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Feature?)null);

        var manager = new FeatureManager(mockStore.Object, Mock.Of<IFeatureEvaluator>());

        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        // Act
        var result = await manager.IsEnabledAsync("MissingFeature", ctx);

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "FeatureManager - IsEnabledAsync should return true when evaluation succeeds")]
    public async Task IsEnabledAsync_ShouldReturnTrue_WhenEvaluationSucceeds()
    {
        // Arrange
        var feature = new Feature("MyFeature", true);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        var mockStore = new Mock<IFeatureStore>();
        mockStore.Setup(s => s.GetAsync("MyFeature", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(feature);

        var mockEngine = new Mock<IFeatureEvaluator>();
        mockEngine.Setup(e => e.EvaluateAsync(feature, ctx, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new EvaluationResult("MyFeature", true));

        var manager = new FeatureManager(mockStore.Object, mockEngine.Object);

        // Act
        var result = await manager.IsEnabledAsync("MyFeature", ctx);

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "FeatureManager - IsEnabledAsync should return false when evaluation fails")]
    public async Task IsEnabledAsync_ShouldReturnFalse_WhenEvaluationFails()
    {
        // Arrange
        var feature = new Feature("MyFeature", true);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        var mockStore = new Mock<IFeatureStore>();
        mockStore.Setup(s => s.GetAsync("MyFeature", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(feature);

        var mockEngine = new Mock<IFeatureEvaluator>();
        mockEngine.Setup(e => e.EvaluateAsync(feature, ctx, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new EvaluationResult("MyFeature", false));

        var manager = new FeatureManager(mockStore.Object, mockEngine.Object);

        // Act
        var result = await manager.IsEnabledAsync("MyFeature", ctx);

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "FeatureManager - EvaluateAsync should return disabled result when feature not found")]
    public async Task EvaluateAsync_ShouldReturnDisabledResult_WhenFeatureNotFound()
    {
        // Arrange
        var mockStore = new Mock<IFeatureStore>();
        mockStore.Setup(s => s.GetAsync("MissingFeature", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Feature?)null);

        var manager = new FeatureManager(mockStore.Object, Mock.Of<IFeatureEvaluator>());

        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        // Act
        var result = await manager.EvaluateAsync("MissingFeature", ctx);

        // Assert
        Assert.Equal("MissingFeature", result.FeatureKey);
        Assert.False(result.Enabled);
    }

    [Fact(DisplayName = "FeatureManager - EvaluateAsync should return result from engine when feature exists")]
    public async Task EvaluateAsync_ShouldReturnResultFromEngine_WhenFeatureExists()
    {
        // Arrange
        var feature = new Feature("ExistingFeature", true);
        var ctx = new FeatureContext("user1", [], new Dictionary<string, object?>());

        var mockStore = new Mock<IFeatureStore>();
        mockStore.Setup(s => s.GetAsync("ExistingFeature", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(feature);

        var expectedResult = new EvaluationResult("ExistingFeature", true);

        var mockEngine = new Mock<IFeatureEvaluator>();
        mockEngine.Setup(e => e.EvaluateAsync(feature, ctx, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(expectedResult);

        var manager = new FeatureManager(mockStore.Object, mockEngine.Object);

        // Act
        var result = await manager.EvaluateAsync("ExistingFeature", ctx);

        // Assert
        Assert.Equal(expectedResult.FeatureKey, result.FeatureKey);
        Assert.Equal(expectedResult.Enabled, result.Enabled);
    }
}