using System.Text.Json;

namespace NotebookMcpServer;

public class NotebookService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public NotebookService(string filePath)
    {
        _filePath = filePath;
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new();
        }

        await using var stream = File.OpenRead(_filePath);
        var data = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, string>>>(stream);
        return data ?? new();
    }

    private async Task SaveAsync(Dictionary<string, Dictionary<string, string>> data)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, data, _options);
    }

    public async Task<IDictionary<string, string>> ViewAsync(string notebookName)
    {
        var all = await LoadAsync();
        if (all.TryGetValue(notebookName, out var entries))
        {
            return new Dictionary<string, string>(entries);
        }

        return new Dictionary<string, string>();
    }

    public async Task WriteAsync(string notebookName, string key, string value)
    {
        var all = await LoadAsync();
        if (!all.TryGetValue(notebookName, out var entries))
        {
            entries = new();
            all[notebookName] = entries;
        }

        entries[key] = value ?? string.Empty;
        await SaveAsync(all);
    }

    public async Task DeleteAsync(string notebookName, string key)
    {
        var all = await LoadAsync();
        if (all.TryGetValue(notebookName, out var entries))
        {
            entries.Remove(key);
        }

        await SaveAsync(all);
    }
}

