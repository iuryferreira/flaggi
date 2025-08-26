using Flaggi.Stores;
using Moq;

namespace Flaggi.Test.Stores;

public class CompositeFeatureStoreTest
{
    [Fact(DisplayName = "CompositeFeatureStore - Constructor should throw when stores is null")]
    public void Constructor_ShouldThrow_WhenStoresIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CompositeFeatureStore(null!));
    }

    [Fact(DisplayName = "CompositeFeatureStore - GetAllAsync should return empty when no stores provided")]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoStoresProvided()
    {
        // Arrange
        var store = new CompositeFeatureStore(Array.Empty<IFeatureStore>());

        // Act
        var (features, etag) = await store.GetAllAsync();

        // Assert
        Assert.Empty(features);
        Assert.Equal(string.Empty, etag);
    }

    [Fact(DisplayName = "CompositeFeatureStore - GetAllAsync should merge features and concatenate ETags")]
    public async Task GetAllAsync_ShouldMergeFeatures_AndConcatenateEtags()
    {
        // Arrange
        var feature1 = new Feature("FeatureX", true);
        var feature2 = new Feature("FeatureY", false);

        var mockStore1 = new Mock<IFeatureStore>();
        mockStore1.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Feature> { feature1 }, "etag1"));

        var mockStore2 = new Mock<IFeatureStore>();
        mockStore2.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Feature> { feature2 }, "etag2"));

        var store = new CompositeFeatureStore([mockStore1.Object, mockStore2.Object]);

        // Act
        var (features, etag) = await store.GetAllAsync();

        // Assert
        Assert.Equal(2, features.Count);
        Assert.Contains(features, f => f.Key == "FeatureX");
        Assert.Contains(features, f => f.Key == "FeatureY");
        Assert.Equal("etag1|etag2", etag);
    }

    [Fact(DisplayName = "CompositeFeatureStore - GetAllAsync should keep last duplicate feature")]
    public async Task GetAllAsync_ShouldKeepLastDuplicateFeature()
    {
        // Arrange
        var featureOld = new Feature("SameKey", false);
        var featureNew = new Feature("SameKey", true);

        var mockStore1 = new Mock<IFeatureStore>();
        mockStore1.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Feature> { featureOld }, "etag1"));

        var mockStore2 = new Mock<IFeatureStore>();
        mockStore2.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Feature> { featureNew }, "etag2"));

        var store = new CompositeFeatureStore([mockStore1.Object, mockStore2.Object]);

        // Act
        var (features, _) = await store.GetAllAsync();

        // Assert
        Assert.Single(features);
        Assert.True(features[0].IsEnabled); // last wins
    }

    [Fact(DisplayName = "CompositeFeatureStore - GetAsync should return null when feature not found")]
    public async Task GetAsync_ShouldReturnNull_WhenFeatureNotFound()
    {
        // Arrange
        var mockStore = new Mock<IFeatureStore>();
        mockStore.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feature?)null);

        var store = new CompositeFeatureStore([mockStore.Object]);

        // Act
        var result = await store.GetAsync("NonExisting");

        // Assert
        Assert.Null(result);
    }

    [Fact(DisplayName = "CompositeFeatureStore - GetAsync should return feature from first matching store")]
    public async Task GetAsync_ShouldReturnFeature_FromFirstMatchingStore()
    {
        // Arrange
        var feature = new Feature("MyFeature", true);

        var mockStore1 = new Mock<IFeatureStore>();
        mockStore1.Setup(s => s.GetAsync("MyFeature", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feature?)null);

        var mockStore2 = new Mock<IFeatureStore>();
        mockStore2.Setup(s => s.GetAsync("MyFeature", It.IsAny<CancellationToken>()))
            .ReturnsAsync(feature);

        var store = new CompositeFeatureStore([mockStore1.Object, mockStore2.Object]);

        // Act
        var result = await store.GetAsync("MyFeature");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyFeature", result!.Key);
        Assert.True(result.IsEnabled);
    }
}
