using ModelContextProtocol.Server;
using NotebookMcpServer.Interfaces;
using NotebookMcpServer.Models;
using System.ComponentModel;

namespace NotebookMcpServer.Tools;

/// <summary>
/// MCP tools for working with notebooks as collections of page/text pairs.
/// Provides read (list pages), upsert, and delete operations.
/// </summary>
[McpServerToolType]
[Description("Tools for viewing, upserting, and deleting pages in named notebooks.")]
public class NotebookTools
{
    private readonly INotebookService _notebookService;

    public NotebookTools(INotebookService notebookService)
    {
        _notebookService = notebookService;
    }

    [McpServerTool(Name = "create_notebook")]
    [Description("Create a notebook or update its description.")]
    public async Task<string> CreateNotebookAsync(
        [Description("Name of the notebook (case-insensitive, non-empty).")]
        string notebookName,
        [Description("Description to store for the notebook.")]
        string description)
    {
        if (string.IsNullOrWhiteSpace(notebookName))
        {
            throw new ArgumentException("Notebook name must be a non-empty string.", nameof(notebookName));
        }

        description ??= string.Empty;
        await _notebookService.CreateNotebookAsync(notebookName, description);
        return $"Notebook '{notebookName}' has been created or updated.";
    }

    /// <summary>
    /// Returns notebook description and page names without page content.
    /// </summary>
    /// <param name="notebookName">Name of the target notebook (case-insensitive, non-empty).</param>
    /// <returns>Notebook summary.</returns>
    [McpServerTool(Name = "get_notebook_page_names")]
    [Description("Get notebook description and page names without page text.")]
    public async Task<NotebookSummary> GetNotebookPageNamesAsync(
            [Description("Name of the notebook to read (case-insensitive, non-empty).")]
            string notebookName)
    {
        if (string.IsNullOrWhiteSpace(notebookName))
        {
            throw new ArgumentException("Notebook name must be a non-empty string.", nameof(notebookName));
        }

        return await _notebookService.ViewNotebookAsync(notebookName);
    }

    /// <summary>
    /// Returns the text of a single page from the specified notebook.
    /// </summary>
    /// <param name="notebookName">Name of the target notebook (case-insensitive, non-empty).</param>
    /// <param name="page">Page name to read (non-empty).</param>
    /// <returns>Text of the page or an empty string if not found.</returns>
    [McpServerTool(Name = "get_page_text")]
    [Description("Read a single page from a notebook.")]
    public async Task<string> GetPageTextAsync(
        [Description("Name of the notebook to read (case-insensitive, non-empty).")]
            string notebookName,
        [Description("Page to read from the notebook (non-empty string).")]
            string page)
    {
        if (string.IsNullOrWhiteSpace(notebookName))
        {
            throw new ArgumentException("Notebook name must be a non-empty string.", nameof(notebookName));
        }

        if (string.IsNullOrWhiteSpace(page))
        {
            throw new ArgumentException("Page must be a non-empty string.", nameof(page));
        }

        return await _notebookService.GetPageAsync(notebookName, page);
    }

    /// <summary>
    /// Creates or overwrites a single page in the specified notebook.
    /// </summary>
    /// <param name="notebookName">Name of the target notebook (case-insensitive, non-empty).</param>
    /// <param name="page">Page name (non-empty; unique within the notebook). Existing text will be overwritten.</param>
    /// <param name="text">Text to store for the page (string, stored verbatim; null is not allowed).</param>
    /// <returns>Operation status as a short human-readable message.</returns>
    [McpServerTool(Name = "upsert_page")]
    [Description("Create or update a page in a notebook.")]
    public async Task<string> UpsertPageAsync(
        [Description("Name of the target notebook (case-insensitive, non-empty).")]
        string notebookName,
        [Description("Page to create or update (non-empty string). Existing text will be overwritten.")]
        string page,
        [Description("Text to store for the page (string, stored verbatim; must not be null).")]
        string text)
    {
        if (string.IsNullOrWhiteSpace(notebookName))
        {
            throw new ArgumentException("Notebook name must be a non-empty string.", nameof(notebookName));
        }

        if (string.IsNullOrWhiteSpace(page))
        {
            throw new ArgumentException("Page must be a non-empty string.", nameof(page));
        }

        if (text is null)
        {
            throw new ArgumentNullException(nameof(text), "Text must not be null. Use an empty string if appropriate.");
        }

        await _notebookService.WritePageAsync(notebookName, page, text);
        return $"Page '{page}' has been upserted in notebook '{notebookName}'.";
    }

    /// <summary>
    /// Deletes a single page from the specified notebook.
    /// </summary>
    /// <param name="notebookName">Name of the target notebook (case-insensitive, non-empty).</param>
    /// <param name="page">Page name to delete (non-empty).</param>
    /// <returns><c>true</c> if the page existed and was deleted; otherwise <c>false</c>.</returns>
    [McpServerTool(Name = "remove_page")]
    [Description("Delete a single page from a notebook. Returns true if deleted, false if not found.")]
    public async Task<bool> RemovePageAsync(
        [Description("Name of the target notebook (case-insensitive, non-empty).")]
        string notebookName,
        [Description("Page to delete from the notebook (non-empty string).")]
        string page)
    {
        if (string.IsNullOrWhiteSpace(notebookName))
        {
            throw new ArgumentException("Notebook name must be a non-empty string.", nameof(notebookName));
        }

        if (string.IsNullOrWhiteSpace(page))
        {
            throw new ArgumentException("Page must be a non-empty string.", nameof(page));
        }

        return await _notebookService.DeletePageAsync(notebookName, page);
    }

}
