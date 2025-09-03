using NotebookMcpServer.Models;

namespace NotebookMcpServer.Interfaces;

/// <summary>
/// Interface for notebook business operations
/// </summary>
public interface INotebookService
{
    /// <summary>
    /// Get notebook description and page titles
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Notebook summary</returns>
    Task<NotebookSummary> ViewNotebookAsync(string notebookName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get text of a single page from a notebook
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="page">Page name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Page text or empty string if not found</returns>
    Task<string> GetPageAsync(string notebookName, string page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write or update a page in a notebook
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="page">Page name</param>
    /// <param name="text">Page text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task WritePageAsync(string notebookName, string page, string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a page from a notebook
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="page">Page name to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the page was deleted, false if it didn't exist</returns>
    Task<bool> DeletePageAsync(string notebookName, string page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a notebook or update its description
    /// </summary>
    /// <param name="notebookName">Name of the notebook</param>
    /// <param name="description">Notebook description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateNotebookAsync(string notebookName, string description, CancellationToken cancellationToken = default);
}
