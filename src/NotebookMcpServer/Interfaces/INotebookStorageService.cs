using NotebookMcpServer.Models;

namespace NotebookMcpServer.Interfaces;

/// <summary>
/// Interface for notebook storage operations
/// </summary>
public interface INotebookStorageService
{
    /// <summary>
    /// Load a notebook by name
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The notebook if found, null otherwise</returns>
    Task<Notebook?> LoadNotebookAsync(string notebookName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save a notebook
    /// </summary>
    /// <param name="notebook">The notebook to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveNotebookAsync(Notebook notebook, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a notebook exists
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the notebook exists</returns>
    Task<bool> NotebookExistsAsync(string notebookName, CancellationToken cancellationToken = default);
}
