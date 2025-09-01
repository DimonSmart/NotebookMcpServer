namespace NotebookMcpServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotebookMcpServer.Interfaces;
using NotebookMcpServer.Services;
using NotebookMcpServer.Tools;

/// <summary>
/// Main program class for the Notebook MCP Server.
/// </summary>
internal sealed class Program
{
    /// <summary>
    /// Entry point for the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Notebook MCP Server...");

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(ConfigureServices)
            .Build();

        await host.RunAsync();
    }


    private static void ConfigureServices(IServiceCollection services)
    {
        // Configure logging
        services.AddLogging(builder => builder.AddConsole());

        // Register storage service
        services.AddSingleton<INotebookStorageService, FileNotebookStorageService>();

        // Register business logic service
        services.AddSingleton<INotebookService, NotebookService>();

        // Configure MCP server
        services.AddMcpServer()
        .WithTools([typeof(NotebookTools)])
        .WithStdioServerTransport();
    }
}
