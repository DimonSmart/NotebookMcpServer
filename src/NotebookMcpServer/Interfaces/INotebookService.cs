using NotebookMcpServer.Models;

namespace NotebookMcpServer.Interfaces;

/// <summary>
/// Interface for notebook business operations
/// </summary>
public interface INotebookService
{
    /// <summary>
    /// Get all entries from a notebook
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of all entries in the notebook</returns>
    Task<Dictionary<string, string>> ViewNotebookAsync(string notebookName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a single entry's value from a notebook
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="key">Entry key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entry value or empty string if not found</returns>
    Task<string> GetEntryAsync(string notebookName, string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Write or update an entry in a notebook
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="key">Entry key</param>
    /// <param name="value">Entry value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task WriteEntryAsync(string notebookName, string key, string value, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete an entry from a notebook
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="key">Entry key to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the entry was deleted, false if it didn't exist</returns>
    Task<bool> DeleteEntryAsync(string notebookName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a notebook or update its description
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="description">Notebook description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateNotebookAsync(string notebookName, string description, CancellationToken cancellationToken = default);
}
