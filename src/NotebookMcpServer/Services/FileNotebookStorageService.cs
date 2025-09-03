using Microsoft.Extensions.Logging;
using NotebookMcpServer.Interfaces;
using NotebookMcpServer.Models;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NotebookMcpServer.Services;

/// <summary>
/// File-based storage with thread-safe access using a single global semaphore for simplicity.
/// Writes directly to files without temporary files since this is a local server.
/// </summary>
public class FileNotebookStorageService : INotebookStorageService, IDisposable
{
    private readonly string _baseDirectory;
    private readonly ILogger<FileNotebookStorageService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    private readonly SemaphoreSlim _globalSemaphore;
    private volatile bool _disposed;

    public FileNotebookStorageService(ILogger<FileNotebookStorageService> logger)
    {
        _logger = logger;
        _baseDirectory = Path.Combine(AppContext.BaseDirectory, "notebooks");
        _globalSemaphore = new SemaphoreSlim(1, 1);

        Directory.CreateDirectory(_baseDirectory);
    }

    private string GetNotebookFilePath(string notebookName)
    {
        var safeNotebookName = string.Join("_", notebookName.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_baseDirectory, $"{safeNotebookName}.json");
    }



    public async Task<Notebook?> LoadNotebookAsync(string notebookName, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var filePath = GetNotebookFilePath(notebookName);

        await _globalSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogDebug("Notebook file does not exist: {FilePath}", filePath);
                return null;
            }

            _logger.LogDebug("Loading notebook from: {FilePath}", filePath);

            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var notebook = await JsonSerializer.DeserializeAsync<Notebook>(stream, JsonOptions, cancellationToken);

            _logger.LogDebug(
                "Successfully loaded notebook '{NotebookName}' with {EntryCount} entries",
                notebookName, notebook?.Entries.Count ?? 0);

            return notebook;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load notebook '{NotebookName}' from {FilePath}", notebookName, filePath);
            throw;
        }
        finally
        {
            _globalSemaphore.Release();
        }
    }

    public async Task SaveNotebookAsync(Notebook notebook, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var filePath = GetNotebookFilePath(notebook.Name);

        await _globalSemaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogDebug("Saving notebook '{NotebookName}' to: {FilePath}", notebook.Name, filePath);

            var notebookToSave = notebook with { ModifiedAt = DateTime.UtcNow };

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, notebookToSave, JsonOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);

            _logger.LogDebug(
                "Successfully saved notebook '{NotebookName}' with {EntryCount} entries",
                notebook.Name, notebook.Entries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save notebook '{NotebookName}' to {FilePath}", notebook.Name, filePath);
            throw;
        }
        finally
        {
            _globalSemaphore.Release();
        }
    }

    public async Task<bool> NotebookExistsAsync(string notebookName, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var filePath = GetNotebookFilePath(notebookName);
        return await Task.FromResult(File.Exists(filePath));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _globalSemaphore.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
