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

        var result = notebook.Entries.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value.Value
        );
        
        _logger.LogInformation("Notebook '{NotebookName}' contains {EntryCount} entries", notebookName, result.Count);
        return result;
    }

    public async Task<string> GetEntryAsync(string notebookName, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notebookName);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _logger.LogInformation("Reading entry '{Key}' from notebook '{NotebookName}'", key, notebookName);

        var notebook = await _storageService.LoadNotebookAsync(notebookName, cancellationToken);

        if (notebook == null)
        {
            _logger.LogInformation("Notebook '{NotebookName}' not found, returning empty value for '{Key}'", notebookName, key);
            return string.Empty;
        }

        if (!notebook.Entries.TryGetValue(key, out var entry))
        {
            _logger.LogInformation("Entry '{Key}' not found in notebook '{NotebookName}', returning empty value", key, notebookName);
            return string.Empty;
        }

        return entry.Value;
    }

    public async Task WriteEntryAsync(string notebookName, string key, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notebookName);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        value ??= string.Empty;
        
        _logger.LogInformation("Writing entry '{Key}' to notebook '{NotebookName}'", key, notebookName);
        
        var notebook = await _storageService.LoadNotebookAsync(notebookName, cancellationToken);
        
        if (notebook == null)
        {
            _logger.LogInformation("Creating new notebook '{NotebookName}'", notebookName);
            notebook = new Notebook { Name = notebookName };
        }

        var now = DateTime.UtcNow;
        var entry = new NotebookEntry
        {
            Key = key,
            Value = value,
            CreatedAt = notebook.Entries.ContainsKey(key) ? notebook.Entries[key].CreatedAt : now,
            ModifiedAt = now
        };

        notebook.Entries[key] = entry;
        
        await _storageService.SaveNotebookAsync(notebook, cancellationToken);
        
        _logger.LogInformation("Successfully wrote entry '{Key}' to notebook '{NotebookName}'", key, notebookName);
    }

    public async Task<bool> DeleteEntryAsync(string notebookName, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notebookName);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        _logger.LogInformation("Deleting entry '{Key}' from notebook '{NotebookName}'", key, notebookName);
        
        var notebook = await _storageService.LoadNotebookAsync(notebookName, cancellationToken);
        
        if (notebook == null)
        {
            _logger.LogInformation("Notebook '{NotebookName}' not found, nothing to delete", notebookName);
            return false;
        }

        if (!notebook.Entries.Remove(key))
        {
            _logger.LogInformation("Entry '{Key}' not found in notebook '{NotebookName}'", key, notebookName);
            return false;
        }

        await _storageService.SaveNotebookAsync(notebook, cancellationToken);
        
        _logger.LogInformation("Successfully deleted entry '{Key}' from notebook '{NotebookName}'", key, notebookName);
        return true;
    }
}
