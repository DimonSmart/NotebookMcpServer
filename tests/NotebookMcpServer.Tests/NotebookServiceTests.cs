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
    public async Task GetPageAsync_NonexistentNotebook_ReturnsEmptyString()
    {
        var (_, notebookService) = CreateServices();

        var result = await notebookService.GetPageAsync("missing", "page");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetPageAsync_NonexistentPage_ReturnsEmptyString()
    {
        var (_, notebookService) = CreateServices();

        await notebookService.WritePageAsync("book", "existing", "text");

        var result = await notebookService.GetPageAsync("book", "missing");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetPageAsync_ExistingPage_ReturnsText()
    {
        var (_, notebookService) = CreateServices();

        await notebookService.WritePageAsync("book", "page", "text");

        var result = await notebookService.GetPageAsync("book", "page");

        Assert.Equal("text", result);
    }

    [Fact]
    public async Task WritePageAsync_NewNotebook_CreatesNotebookAndPage()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string page = "test-page";
        const string text = "test-text";

        // Act
        await notebookService.WritePageAsync(notebookName, page, text);

        // Assert
        var result = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Single(result);
        Assert.Equal(text, result[page]);
    }

    [Fact]
    public async Task WritePageAsync_ExistingPage_UpdatesPage()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string page = "test-page";
        const string originalText = "original-text";
        const string updatedText = "updated-text";

        // Act
        await notebookService.WritePageAsync(notebookName, page, originalText);
        await notebookService.WritePageAsync(notebookName, page, updatedText);

        // Assert
        var result = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Single(result);
        Assert.Equal(updatedText, result[page]);
    }

    [Fact]
    public async Task DeletePageAsync_ExistingPage_DeletesPageAndReturnsTrue()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string page = "test-page";
        const string text = "test-text";

        await notebookService.WritePageAsync(notebookName, page, text);

        // Act
        var result = await notebookService.DeletePageAsync(notebookName, page);

        // Assert
        Assert.True(result);
        var pages = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Empty(pages);
    }

    [Fact]
    public async Task DeletePageAsync_NonExistentPage_ReturnsFalse()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string page = "nonexistent-page";

        // Act
        var result = await notebookService.DeletePageAsync(notebookName, page);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeletePageAsync_NonExistentNotebook_ReturnsFalse()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "nonexistent-notebook";
        const string page = "test-page";

        // Act
        var result = await notebookService.DeletePageAsync(notebookName, page);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task WritePageAsync_MultiplePages_StoresAllPages()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        var pages = new Dictionary<string, string>
        {
            ["page1"] = "text1",
            ["page2"] = "text2",
            ["page3"] = "text3"
        };

        // Act
        foreach (var (page, text) in pages)
        {
            await notebookService.WritePageAsync(notebookName, page, text);
        }

        // Assert
        var result = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Equal(pages.Count, result.Count);
        foreach (var (page, expectedText) in pages)
        {
            Assert.Equal(expectedText, result[page]);
        }
    }

    [Fact]
    public async Task CreateNotebookAsync_NewNotebook_SetsDescription()
    {
        var (storageService, notebookService) = CreateServices();

        await notebookService.CreateNotebookAsync("book", "описание");

        var notebook = await storageService.LoadNotebookAsync("book");
        Assert.Equal("описание", notebook?.Description);
    }

    [Fact]
    public async Task CreateNotebookAsync_ExistingNotebook_UpdatesDescription()
    {
        var (storageService, notebookService) = CreateServices();

        await notebookService.CreateNotebookAsync("book", "first");
        await notebookService.CreateNotebookAsync("book", "second");

        var notebook = await storageService.LoadNotebookAsync("book");
        Assert.Equal("second", notebook?.Description);
    }

    [Fact]
    public async Task WritePageAsync_NullNotebookName_ThrowsArgumentException()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => notebookService.WritePageAsync(null!, "page", "text"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task WritePageAsync_InvalidNotebookName_ThrowsArgumentException(string invalidName)
    {
        var (storageService, notebookService) = CreateServices();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => notebookService.WritePageAsync(invalidName, "page", "text"));
    }

    [Fact]
    public async Task WritePageAsync_NullPage_ThrowsArgumentException()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => notebookService.WritePageAsync("notebook", null!, "text"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task WritePageAsync_InvalidPage_ThrowsArgumentException(string invalidPage)
    {
        var (storageService, notebookService) = CreateServices();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => notebookService.WritePageAsync("notebook", invalidPage, "text"));
    }

    [Fact]
    public async Task WritePageAsync_NullText_TreatedAsEmptyString()
    {
        var (storageService, notebookService) = CreateServices();
        
        // Arrange
        const string notebookName = "test-notebook";
        const string page = "test-page";

        // Act
        await notebookService.WritePageAsync(notebookName, page, null!);

        // Assert
        var result = await notebookService.ViewNotebookAsync(notebookName);
        Assert.Single(result);
        Assert.Equal("", result[page]);
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

