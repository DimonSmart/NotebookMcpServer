namespace NotebookMcpServer.Models;

/// <summary>
/// Page with text in a notebook with timestamps.
/// </summary>
public record NotebookPage
{
    public required string Page { get; init; }
    public required string Text { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; init; } = DateTime.UtcNow;
}
