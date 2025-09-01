namespace NotebookMcpServer.Models;

/// <summary>
/// Key-value entry in a notebook with timestamps.
/// </summary>
public record NotebookEntry
{
    public required string Key { get; init; }
    public required string Value { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; init; } = DateTime.UtcNow;
}
