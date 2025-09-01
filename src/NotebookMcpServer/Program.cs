using NotebookMcpServer;

var dataPath = Path.Combine(AppContext.BaseDirectory, "notebooks.json");
var service = new NotebookService(dataPath);

if (args.Length == 0)
{
    Console.WriteLine("Usage: view <notebook> | write <notebook> <key> [value] | delete <notebook> <key>");
    return;
}

switch (args[0].ToLowerInvariant())
{
    case "view":
        if (args.Length < 2)
        {
            Console.WriteLine("Notebook name required");
            return;
        }
        var entries = await service.ViewAsync(args[1]);
        foreach (var pair in entries)
        {
            Console.WriteLine($"{pair.Key}={pair.Value}");
        }
        break;
    case "write":
        if (args.Length < 3)
        {
            Console.WriteLine("Notebook name and key required");
            return;
        }
        var value = args.Length > 3 ? args[3] : string.Empty;
        await service.WriteAsync(args[1], args[2], value);
        break;
    case "delete":
        if (args.Length < 3)
        {
            Console.WriteLine("Notebook name and key required");
            return;
        }
        await service.DeleteAsync(args[1], args[2]);
        break;
    default:
        Console.WriteLine("Unknown command");
        break;
}
