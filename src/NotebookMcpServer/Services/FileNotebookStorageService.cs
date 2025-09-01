using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotebookMcpServer.Interfaces;
using NotebookMcpServer.Models;

namespace NotebookMcpServer.Services;

/// <summary>
/// File-based storage with thread-safe access per notebook using semaphores.
/// </summary>
public class FileNotebookStorageService : INotebookStorageService, IDisposable
{
    private readonly string baseDirectory;
    private readonly ILogger<FileNotebookStorageService> logger;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> notebookSemaphores;
    private volatile bool disposed;

    public FileNotebookStorageService(ILogger<FileNotebookStorageService> logger)
    {
        this.logger = logger;
        baseDirectory = Path.Combine(AppContext.BaseDirectory, "notebooks");
        jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        notebookSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        Directory.CreateDirectory(baseDirectory);
    }

    private string GetNotebookFilePath(string notebookName)
    {
        var safeNotebookName = string.Join("_", notebookName.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(baseDirectory, $"{safeNotebookName}.json");
    }

    private SemaphoreSlim GetSemaphore(string notebookName)
    {
        return notebookSemaphores.GetOrAdd(notebookName, _ => new SemaphoreSlim(1, 1));
    }

    public async Task<Notebook?> LoadNotebookAsync(string notebookName, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var semaphore = GetSemaphore(notebookName);
        var filePath = GetNotebookFilePath(notebookName);

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(filePath))
            {
                logger.LogDebug("Notebook file does not exist: {FilePath}", filePath);
                return null;
            }

            logger.LogDebug("Loading notebook from: {FilePath}", filePath);

            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var notebook = await JsonSerializer.DeserializeAsync<Notebook>(stream, jsonOptions, cancellationToken);

            logger.LogDebug(
                "Successfully loaded notebook '{NotebookName}' with {EntryCount} entries",
                notebookName, notebook?.Entries.Count ?? 0);

            return notebook;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load notebook '{NotebookName}' from {FilePath}", notebookName, filePath);
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task SaveNotebookAsync(Notebook notebook, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var semaphore = GetSemaphore(notebook.Name);
        var filePath = GetNotebookFilePath(notebook.Name);

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            logger.LogDebug("Saving notebook '{NotebookName}' to: {FilePath}", notebook.Name, filePath);

            var notebookToSave = notebook with { ModifiedAt = DateTime.UtcNow };

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempFilePath = filePath + ".tmp";

            await using var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, notebookToSave, jsonOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);

            File.Move(tempFilePath, filePath, true);

            logger.LogDebug(
                "Successfully saved notebook '{NotebookName}' with {EntryCount} entries",
                notebook.Name, notebook.Entries.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save notebook '{NotebookName}' to {FilePath}", notebook.Name, filePath);
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<bool> NotebookExistsAsync(string notebookName, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var filePath = GetNotebookFilePath(notebookName);
        return await Task.FromResult(File.Exists(filePath));
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        foreach (var semaphore in notebookSemaphores.Values)
        {
            semaphore.Dispose();
        }

        notebookSemaphores.Clear();
        disposed = true;
        GC.SuppressFinalize(this);
    }
}
