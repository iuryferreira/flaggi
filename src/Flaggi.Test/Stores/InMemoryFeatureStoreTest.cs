using Flaggi.Stores;

namespace Flaggi.Test.Stores;

public class InMemoryFeatureStoreTest
{
    [Fact(DisplayName = "InMemoryFeatureStore - Constructor should create empty dictionary when features is null")]
    public async Task Constructor_ShouldCreateEmptyDictionary_WhenFeaturesIsNull()
    {
        // Arrange
        var store = new InMemoryFeatureStore(null!);

        // Act
        var (features, etag) = await store.GetAllAsync();

        // Assert
        Assert.Empty(features);
        Assert.False(string.IsNullOrWhiteSpace(etag));
    }

    [Fact(DisplayName = "InMemoryFeatureStore - GetAllAsync should return all features and a non-empty ETag")]
    public async Task GetAllAsync_ShouldReturnAllFeatures_AndNonEmptyETag()
    {
        // Arrange
        var features = new[]
        {
            new Feature("FeatureA", true),
            new Feature("FeatureB", false)
        };
        var store = new InMemoryFeatureStore(features);

        // Act
        var (result, etag) = await store.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.Key == "FeatureA");
        Assert.Contains(result, f => f.Key == "FeatureB");
        Assert.False(string.IsNullOrWhiteSpace(etag));
    }

    [Fact(DisplayName = "InMemoryFeatureStore - GetAsync should return null when feature does not exist")]
    public async Task GetAsync_ShouldReturnNull_WhenFeatureDoesNotExist()
    {
        // Arrange
        var features = new[] { new Feature("Existing", true) };
        var store = new InMemoryFeatureStore(features);

        // Act
        var result = await store.GetAsync("NonExisting");

        // Assert
        Assert.Null(result);
    }

    [Fact(DisplayName = "InMemoryFeatureStore - GetAsync should return feature when it exists (case-insensitive)")]
    public async Task GetAsync_ShouldReturnFeature_WhenItExists_CaseInsensitive()
    {
        // Arrange
        var features = new[] { new Feature("MyFeature", true) };
        var store = new InMemoryFeatureStore(features);

        // Act
        var result1 = await store.GetAsync("MyFeature");
        var result2 = await store.GetAsync("myfeature");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal("MyFeature", result1!.Key);
        Assert.Equal("MyFeature", result2!.Key);
    }
}