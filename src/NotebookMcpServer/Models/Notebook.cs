namespace NotebookMcpServer.Models;

/// <summary>
/// Complete notebook with pages and timestamps.
/// </summary>
public record Notebook
{
    public required string Name { get; init; }
    public string? Description { get; set; }
    public Dictionary<string, NotebookPage> Pages { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
