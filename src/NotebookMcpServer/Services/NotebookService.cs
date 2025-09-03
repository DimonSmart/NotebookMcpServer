using Microsoft.Extensions.Logging;
using NotebookMcpServer.Interfaces;
using NotebookMcpServer.Models;

namespace NotebookMcpServer.Services;

/// <summary>
/// Business logic service for notebook operations
/// </summary>
public class NotebookService : INotebookService
{
    private readonly INotebookStorageService _storageService;
    private readonly ILogger<NotebookService> _logger;

    public NotebookService(INotebookStorageService storageService, ILogger<NotebookService> logger)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Dictionary<string, string>> ViewNotebookAsync(string notebookName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notebookName);

        _logger.LogInformation("Viewing notebook '{NotebookName}'", notebookName);

        var notebook = await _storageService.LoadNotebookAsync(notebookName, cancellationToken);

        if (notebook == null)
        {
            _logger.LogInformation("Notebook '{NotebookName}' not found, returning empty result", notebookName);
            return new Dictionary<string, string>();
        }

        var result = notebook.Pages.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Text
        );

        _logger.LogInformation("Notebook '{NotebookName}' contains {PageCount} pages", notebookName, result.Count);
        return result;
    }

    public async Task<string> GetPageAsync(string notebookName, string page, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notebookName);
        ArgumentException.ThrowIfNullOrWhiteSpace(page);

        _logger.LogInformation("Reading page '{Page}' from notebook '{NotebookName}'", page, notebookName);

        var notebook = await _storageService.LoadNotebookAsync(notebookName, cancellationToken);

        if (notebook == null)
        {
            _logger.LogInformation("Notebook '{NotebookName}' not found, returning empty text for '{Page}'", notebookName, page);
            return string.Empty;
        }

        if (!notebook.Pages.TryGetValue(page, out var pageEntry))
        {
            _logger.LogInformation("Page '{Page}' not found in notebook '{NotebookName}', returning empty text", page, notebookName);
            return string.Empty;
        }

        return pageEntry.Text;
    }

    public async Task WritePageAsync(string notebookName, string page, string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notebookName);
        ArgumentException.ThrowIfNullOrWhiteSpace(page);
        text ??= string.Empty;

        _logger.LogInformation("Writing page '{Page}' to notebook '{NotebookName}'", page, notebookName);

        var notebook = await _storageService.LoadNotebookAsync(notebookName, cancellationToken);

        if (notebook == null)
        {
            _logger.LogInformation("Creating new notebook '{NotebookName}'", notebookName);
            notebook = new Notebook { Name = notebookName };
        }

        var now = DateTime.UtcNow;
        var pageData = new NotebookPage
        {
            Page = page,
            Text = text,
            CreatedAt = notebook.Pages.ContainsKey(page) ? notebook.Pages[page].CreatedAt : now,
            ModifiedAt = now
        };

        notebook.Pages[page] = pageData;

        await _storageService.SaveNotebookAsync(notebook, cancellationToken);

        _logger.LogInformation("Successfully wrote page '{Page}' to notebook '{NotebookName}'", page, notebookName);
    }

    public async Task<bool> DeletePageAsync(string notebookName, string page, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notebookName);
        ArgumentException.ThrowIfNullOrWhiteSpace(page);

        _logger.LogInformation("Deleting page '{Page}' from notebook '{NotebookName}'", page, notebookName);

        var notebook = await _storageService.LoadNotebookAsync(notebookName, cancellationToken);

        if (notebook == null)
        {
            _logger.LogInformation("Notebook '{NotebookName}' not found, nothing to delete", notebookName);
            return false;
        }

        if (!notebook.Pages.Remove(page))
        {
            _logger.LogInformation("Page '{Page}' not found in notebook '{NotebookName}'", page, notebookName);
            return false;
        }

        await _storageService.SaveNotebookAsync(notebook, cancellationToken);

        _logger.LogInformation("Successfully deleted page '{Page}' from notebook '{NotebookName}'", page, notebookName);
        return true;
    }

    public async Task CreateNotebookAsync(string notebookName, string description, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notebookName);
        description ??= string.Empty;

        _logger.LogInformation("Creating or updating notebook '{NotebookName}'", notebookName);

        var notebook = await _storageService.LoadNotebookAsync(notebookName, cancellationToken) ?? new Notebook { Name = notebookName };
        notebook.Description = description;

        await _storageService.SaveNotebookAsync(notebook, cancellationToken);

        _logger.LogInformation("Notebook '{NotebookName}' saved with description", notebookName);
    }
}
