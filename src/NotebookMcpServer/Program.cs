namespace NotebookMcpServer;

using Microsoft.Extensions.Configuration;
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
            .ConfigureAppConfiguration((ctx, cfg) =>
            {
                cfg.AddEnvironmentVariables(prefix: "NOTEBOOK_");
            })
            .ConfigureServices(ConfigureServices)
            .Build();

        // Log version and startup info
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
        logger.LogInformation("Notebook MCP Server version {Version} started", version);

        await host.RunAsync();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var configuration = context.Configuration;
        
        // Get storage directory from configuration or environment variable
        var storageDir = configuration["Storage:Directory"] ??
                         Environment.GetEnvironmentVariable("NOTEBOOK_STORAGE_DIRECTORY") ??
                         Path.Combine(AppContext.BaseDirectory, "notebooks");

        // Configure logging
        services.AddLogging(builder => builder.AddConsole());

        // Register storage service with configurable directory
        services.AddSingleton<INotebookStorageService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FileNotebookStorageService>>();
            return new FileNotebookStorageService(storageDir, logger);
        });

        // Register business logic service
        services.AddSingleton<INotebookService, NotebookService>();

        // Configure MCP server
        services.AddMcpServer()
        .WithTools([typeof(NotebookTools)])
        .WithStdioServerTransport();
    }
}
