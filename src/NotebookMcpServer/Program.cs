namespace NotebookMcpServer;

using System.Reflection;
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
        // Handle --version or -v
        if (args.Any(a => a is "--version" or "-v"))
        {
            var asm = Assembly.GetExecutingAssembly();
            var versionInfo =
                asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? asm.GetName().Version?.ToString()
                ?? "unknown";

            Console.WriteLine($"NotebookMcpServer {versionInfo}");
            return;
        }

        // Handle --help or -h
        if (args.Any(a => a is "--help" or "-h"))
        {
            Console.WriteLine("Notebook MCP Server");
            Console.WriteLine();
            Console.WriteLine("A Model Context Protocol (MCP) server for managing notebooks with pages of text and persistent file storage.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  NotebookMcpServer [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --version, -v    Show version information");
            Console.WriteLine("  --help, -h       Show this help message");
            Console.WriteLine();
            Console.WriteLine("Configuration:");
            Console.WriteLine("  NOTEBOOK_STORAGE_DIRECTORY    Directory for storing notebook files (default: ./notebooks)");
            Console.WriteLine();
            Console.WriteLine("The server listens on stdin/stdout for MCP protocol messages.");
            return;
        }

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
            return new FileNotebookStorageService(logger);
        });

        // Register business logic service
        services.AddSingleton<INotebookService, NotebookService>();

        // Configure MCP server
        services.AddMcpServer()
        .WithTools([typeof(NotebookTools)])
        .WithStdioServerTransport();
    }
}
