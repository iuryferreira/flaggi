using System.Text.Json;
using Flaggi.Stores;

namespace Flaggi.Test.Stores;

public class JsonFileFeatureStoreTest
{
    [Fact(DisplayName = "JsonFileFeatureStore - Constructor should throw when filePath is null or empty")]
    public void Constructor_ShouldThrow_WhenFilePathIsNullOrEmpty()
    {
        Assert.Throws<ArgumentException>(() => new JsonFileFeatureStore(null!));
        Assert.Throws<ArgumentException>(() => new JsonFileFeatureStore(""));
        Assert.Throws<ArgumentException>(() => new JsonFileFeatureStore("   "));
    }

    [Fact(DisplayName = "JsonFileFeatureStore - GetAllAsync should return empty when file does not exist")]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenFileDoesNotExist()
    {
        // Arrange
        var store = new JsonFileFeatureStore("nonexistent.json");

        // Act
        var (features, etag) = await store.GetAllAsync();

        // Assert
        Assert.Empty(features);
        Assert.Equal("missing-file", etag);
    }

    [Fact(DisplayName = "JsonFileFeatureStore - GetAllAsync should return empty list when JSON has no features")]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenJsonHasNoFeatures()
    {
        // Arrange
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "{}");

        var store = new JsonFileFeatureStore(path);

        // Act
        var (features, etag) = await store.GetAllAsync();

        // Assert
        Assert.Empty(features);
        Assert.NotNull(etag);
    }

    [Fact(DisplayName = "JsonFileFeatureStore - GetAllAsync should deserialize features correctly")]
    public async Task GetAllAsync_ShouldDeserializeFeaturesCorrectly()
    {
        // Arrange
        var path = Path.GetTempFileName();
        var feature = new Feature("TestFeature", true);
        var json = JsonSerializer.Serialize(new { Features = new[] { feature } });
        await File.WriteAllTextAsync(path, json);

        var store = new JsonFileFeatureStore(path);

        // Act
        var (features, etag) = await store.GetAllAsync();

        // Assert
        Assert.Single(features);
        Assert.Equal("TestFeature", features[0].Key);
        Assert.True(features[0].IsEnabled);
        Assert.False(string.IsNullOrEmpty(etag));
    }

    [Fact(DisplayName = "JsonFileFeatureStore - GetAsync should return null when feature does not exist")]
    public async Task GetAsync_ShouldReturnNull_WhenFeatureDoesNotExist()
    {
        // Arrange
        var path = Path.GetTempFileName();
        var feature = new Feature("OtherFeature", true);
        var json = JsonSerializer.Serialize(new { Features = new[] { feature } });
        await File.WriteAllTextAsync(path, json);

        var store = new JsonFileFeatureStore(path);

        // Act
        var result = await store.GetAsync("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact(DisplayName = "JsonFileFeatureStore - GetAsync should return feature when it exists (case-insensitive)")]
    public async Task GetAsync_ShouldReturnFeature_WhenItExists_CaseInsensitive()
    {
        // Arrange
        var path = Path.GetTempFileName();
        var feature = new Feature("MyFeature", true);
        var json = JsonSerializer.Serialize(new { Features = new[] { feature } });
        await File.WriteAllTextAsync(path, json);

        var store = new JsonFileFeatureStore(path);

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
