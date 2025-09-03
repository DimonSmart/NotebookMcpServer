namespace NotebookMcpServer.Models;

/// <summary>
/// Notebook description with page titles.
/// </summary>
public record NotebookSummary
{
    public string? Description { get; init; }
    public List<string> Pages { get; init; } = [];
}
