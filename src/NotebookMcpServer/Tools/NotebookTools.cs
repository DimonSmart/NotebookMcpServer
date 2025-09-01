using System.ComponentModel;
using NotebookMcpServer.Interfaces;
using ModelContextProtocol;
using ModelContextProtocol.Server;


namespace NotebookMcpServer.Tools;


[McpServerToolType]
public class NotebookTools
{
    private readonly INotebookService _notebookService;

    public NotebookTools(INotebookService notebookService)
    {
        _notebookService = notebookService;
    }

    [McpServerTool(Name = "view_notebook")]
    public async Task<Dictionary<string, string>> ViewNotebookAsync(
        [Description("Name of the notebook to view")] string notebookName)
    {
        return await _notebookService.ViewNotebookAsync(notebookName);
    }

    [McpServerTool(Name = "write_entry")]
    public async Task<string> WriteEntryAsync(
        [Description("Name of the notebook")] string notebookName,
        [Description("The key to store/update")] string key,
        [Description("The value to store")] string value)
    {
        await _notebookService.WriteEntryAsync(notebookName, key, value);
        return $"Successfully wrote entry '{key}' to notebook '{notebookName}'";
    }

    [McpServerTool(Name = "delete_entry")]
    public async Task<bool> DeleteEntryAsync(
        [Description("Name of the notebook")] string notebookName,
        [Description("The key to delete")] string key)
    {
        return await _notebookService.DeleteEntryAsync(notebookName, key);
    }
}
