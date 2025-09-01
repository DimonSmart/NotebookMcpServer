using NotebookMcpServer;

namespace NotebookMcpServer.Tests;

public class NotebookServiceTests
{
    [Fact]
    public async Task WriteAndView()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var service = new NotebookService(path);
        await service.WriteAsync("work", "task", "done");
        var entries = await service.ViewAsync("work");
        Assert.Equal("done", entries["task"]);
    }

    [Fact]
    public async Task EmptyValuePreserved()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var service = new NotebookService(path);
        await service.WriteAsync("todo", "item", string.Empty);
        var entries = await service.ViewAsync("todo");
        Assert.True(entries.ContainsKey("item"));
        Assert.Equal(string.Empty, entries["item"]);
    }

    [Fact]
    public async Task DeleteRemovesKey()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var service = new NotebookService(path);
        await service.WriteAsync("trash", "old", "value");
        await service.DeleteAsync("trash", "old");
        var entries = await service.ViewAsync("trash");
        Assert.False(entries.ContainsKey("old"));
    }
}

