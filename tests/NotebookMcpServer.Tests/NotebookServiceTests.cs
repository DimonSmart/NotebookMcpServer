using Microsoft.Extensions.Logging;
using NotebookMcpServer.Interfaces;
using NotebookMcpServer.Models;
using NotebookMcpServer.Services;

namespace NotebookMcpServer.Tests;

public class NotebookServiceTests
{
    private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
    
    private (INotebookStorageService storageService, NotebookService notebookService) CreateServices()
    {
        var serviceLogger = LoggerFactory.CreateLogger<NotebookService>();
        
        // Create an in-memory storage service for testing
        var storageService = new InMemoryNotebookStorageService();
        var notebookService = new NotebookService(storageService, serviceLogger);
        
        return (storageService, notebookService);
    }

    [Fact]
    public async Task ViewNotebookAsync_NewNotebook_ReturnsEmptyDictionary()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Act
        var result = await notebookService.ViewNotebookAsync("nonexistent");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEntryAsync_NonexistentNotebook_ReturnsEmptyString()
    {
        var (_, notebookService) = CreateServices();

        var result = await notebookService.GetEntryAsync("missing", "key");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetEntryAsync_NonexistentKey_ReturnsEmptyString()
    {
        var (_, notebookService) = CreateServices();

        await notebookService.WriteEntryAsync("book", "existing", "value");

        var result = await notebookService.GetEntryAsync("book", "missing");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetEntryAsync_ExistingEntry_ReturnsValue()
    {
        var (_, notebookService) = CreateServices();

        await notebookService.WriteEntryAsync("book", "key", "value");

        var result = await notebookService.GetEntryAsync("book", "key");

        Assert.Equal("value", result);
    }

    [Fact]
    public async Task WriteEntryAsync_NewNotebook_CreatesNotebookAndEntry()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string key = "test-key";
        const string value = "test-value";

        // Act
        await notebookService.WriteEntryAsync(notebookName, key, value);

        // Assert
        var result = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Single(result);
        Assert.Equal(value, result[key]);
    }

    [Fact]
    public async Task WriteEntryAsync_ExistingKey_UpdatesEntry()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string key = "test-key";
        const string originalValue = "original-value";
        const string updatedValue = "updated-value";

        // Act
        await notebookService.WriteEntryAsync(notebookName, key, originalValue);
        await notebookService.WriteEntryAsync(notebookName, key, updatedValue);

        // Assert
        var result = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Single(result);
        Assert.Equal(updatedValue, result[key]);
    }

    [Fact]
    public async Task DeleteEntryAsync_ExistingEntry_DeletesEntryAndReturnsTrue()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string key = "test-key";
        const string value = "test-value";

        await notebookService.WriteEntryAsync(notebookName, key, value);

        // Act
        var result = await notebookService.DeleteEntryAsync(notebookName, key);

        // Assert
        Assert.True(result);
        var entries = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Empty(entries);
    }

    [Fact]
    public async Task DeleteEntryAsync_NonExistentEntry_ReturnsFalse()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string key = "nonexistent-key";

        // Act
        var result = await notebookService.DeleteEntryAsync(notebookName, key);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteEntryAsync_NonExistentNotebook_ReturnsFalse()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "nonexistent-notebook";
        const string key = "test-key";

        // Act
        var result = await notebookService.DeleteEntryAsync(notebookName, key);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task WriteEntryAsync_EmptyValue_StoresEmptyString()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string key = "test-key";

        // Act
        await notebookService.WriteEntryAsync(notebookName, key, "");

        // Assert
        var result = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Single(result);
        Assert.Equal("", result[key]);
    }

    [Fact]
    public async Task WriteEntryAsync_MultipleEntries_StoresAllEntries()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        var entries = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
            ["key3"] = "value3"
        };

        // Act
        foreach (var (key, value) in entries)
        {
            await notebookService.WriteEntryAsync(notebookName, key, value);
        }

        // Assert
        var result = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Equal(entries.Count, result.Count);
        foreach (var (key, expectedValue) in entries)
        {
            Assert.Equal(expectedValue, result[key]);
        }
    }

    [Fact]
    public async Task WriteEntryAsync_NullNotebookName_ThrowsArgumentException()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => notebookService.WriteEntryAsync(null!, "key", "value"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task WriteEntryAsync_InvalidNotebookName_ThrowsArgumentException(string invalidName)
    {
        var (storageService, notebookService) = CreateServices();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => notebookService.WriteEntryAsync(invalidName, "key", "value"));
    }

    [Fact]
    public async Task WriteEntryAsync_NullKey_ThrowsArgumentException()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => notebookService.WriteEntryAsync("notebook", null!, "value"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task WriteEntryAsync_InvalidKey_ThrowsArgumentException(string invalidKey)
    {
        var (storageService, notebookService) = CreateServices();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => notebookService.WriteEntryAsync("notebook", invalidKey, "value"));
    }

    [Fact]
    public async Task WriteEntryAsync_NullValue_TreatedAsEmptyString()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string key = "test-key";

        // Act
        await notebookService.WriteEntryAsync(notebookName, key, null!);

        // Assert
        var result = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Single(result);
        Assert.Equal("", result[key]);
    }
}

// Test-specific implementation that works in memory
internal class InMemoryNotebookStorageService : INotebookStorageService
{
    private readonly Dictionary<string, Notebook> _notebooks = new();

    public Task<Notebook?> LoadNotebookAsync(string notebookName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_notebooks.TryGetValue(notebookName, out var notebook) ? notebook : null);
    }

    public Task SaveNotebookAsync(Notebook notebook, CancellationToken cancellationToken = default)
    {
        _notebooks[notebook.Name] = notebook;
        return Task.CompletedTask;
    }

    public Task<bool> NotebookExistsAsync(string notebookName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_notebooks.ContainsKey(notebookName));
    }
}

