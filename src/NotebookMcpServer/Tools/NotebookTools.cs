using ModelContextProtocol.Server;
using NotebookMcpServer.Interfaces;
using System.ComponentModel;

namespace NotebookMcpServer.Tools;

/// <summary>
/// MCP tools for working with "notebooks" — named collections of string key/value pairs.
/// Provides read (list entries), upsert (create or overwrite), and delete operations.
/// </summary>
[McpServerToolType]
[Description("Tools for viewing, upserting, and deleting string key/value entries in named notebooks.")]
public class NotebookTools
{
    private readonly INotebookService _notebookService;

    public NotebookTools(INotebookService notebookService)
    {
        _notebookService = notebookService;
    }

    /// <summary>
    /// Returns all entries of the specified notebook as a dictionary (key → value).
    /// </summary>
    /// <param name="notebookName">Exact name of the target notebook (case‑sensitive, non-empty).</param>
    /// <returns>A dictionary of current entries in the notebook.</returns>
    [McpServerTool(Name = "get_notebook_entries")]
    [Description("List all key/value entries in the specified notebook. Example: { \"notebookName\": \"spanish\" }")]
    public async Task<Dictionary<string, string>> GetNotebookEntriesAsync(
        [Description("Exact name of the notebook to read (case‑sensitive, non-empty).")]
        string notebookName)
    {
        if (string.IsNullOrWhiteSpace(notebookName))
        {
            throw new ArgumentException("Notebook name must be a non-empty string.", nameof(notebookName));
        }

        return await _notebookService.ViewNotebookAsync(notebookName);
    }

    /// <summary>
    /// Creates or overwrites a single entry (key → value) in the specified notebook.
    /// </summary>
    /// <param name="notebookName">Exact name of the target notebook (case‑sensitive, non-empty).</param>
    /// <param name="key">Entry key (non-empty; unique within the notebook). Existing value will be overwritten.</param>
    /// <param name="value">Entry value to store (string, stored verbatim; null is not allowed).</param>
    /// <returns>Operation status as a short human-readable message.</returns>
    [McpServerTool(Name = "upsert_entry")]
    [Description("Create or update a key/value entry in a notebook. Example: { \"notebookName\": \"spanish\", \"key\": \"acogedor\", \"value\": \"cozy, welcoming\" }")]
    public async Task<string> UpsertEntryAsync(
        [Description("Exact name of the target notebook (case‑sensitive, non-empty).")]
        string notebookName,
        [Description("Key to create or update (non-empty string). Existing value will be overwritten.")]
        string key,
        [Description("Value to store for the key (string, stored verbatim; must not be null).")]
        string value)
    {
        if (string.IsNullOrWhiteSpace(notebookName))
        {
            throw new ArgumentException("Notebook name must be a non-empty string.", nameof(notebookName));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must be a non-empty string.", nameof(key));
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Value must not be null. Use an empty string if appropriate.");
        }

        await _notebookService.WriteEntryAsync(notebookName, key, value);
        return $"Entry '{key}' has been upserted in notebook '{notebookName}'.";
    }

    /// <summary>
    /// Deletes a single entry from the specified notebook.
    /// </summary>
    /// <param name="notebookName">Exact name of the target notebook (case‑sensitive, non-empty).</param>
    /// <param name="key">Key of the entry to delete (non-empty).</param>
    /// <returns><c>true</c> if the entry existed and was deleted; otherwise <c>false</c>.</returns>
    [McpServerTool(Name = "remove_entry")]
    [Description("Delete a single key from a notebook. Returns true if deleted, false if not found. Example: { \"notebookName\": \"spanish\", \"key\": \"acogedor\" }")]
    public async Task<bool> RemoveEntryAsync(
        [Description("Exact name of the target notebook (case‑sensitive, non-empty).")]
        string notebookName,
        [Description("Key to delete from the notebook (non-empty string).")]
        string key)
    {
        if (string.IsNullOrWhiteSpace(notebookName))
        {
            throw new ArgumentException("Notebook name must be a non-empty string.", nameof(notebookName));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must be a non-empty string.", nameof(key));
        }

        return await _notebookService.DeleteEntryAsync(notebookName, key);
    }

}
