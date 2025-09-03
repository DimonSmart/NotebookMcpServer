# Notebook MCP Server

A Model Context Protocol (MCP) server for managing notebooks with pages of text and persistent file storage.

## Overview

This MCP server provides tools for creating, viewing, updating, and managing notebooks that store page/text pairs. Each notebook is stored as a separate JSON file, with thread-safe operations and proper concurrency control.

## Features

- **Page Storage**: Store and retrieve pages with text in named notebooks
- **Persistent Storage**: Data is stored in JSON files for persistence
- **Thread-Safe Operations**: Uses semaphores for concurrent access protection
- **Separate Files**: Each notebook is stored in its own JSON file
- **MCPSharp Framework**: Built using MCPSharp v1.0.11 (modern MCP implementation)
- **Dependency Injection**: Proper DI architecture using Microsoft.Extensions.DependencyInjection
- **Comprehensive Testing**: Full unit test coverage

## Available Tools

### 1. Get Notebook Pages (`get_notebook_pages`)
Retrieves all pages with their text from a notebook.

**Parameters:**
- `notebook_name` (string): Name of the notebook to view

**Returns:** Dictionary of page-text pairs

**Example:**
```json
{
  "notebook_name": "my-notebook"
}
```

### 2. Upsert Page (`upsert_page`)
Creates or updates a page in a notebook.

**Parameters:**
- `notebook_name` (string): Name of the notebook
- `page` (string): Page name to store/update
- `text` (string): Text to store on the page

**Returns:** Success confirmation

**Example:**
```json
{
  "notebook_name": "my-notebook",
  "page": "important-note",
  "text": "Remember to update documentation"
}
```

### 3. Remove Page (`remove_page`)
Deletes a specific page from a notebook.

**Parameters:**
- `notebook_name` (string): Name of the notebook
- `page` (string): The page to delete

**Returns:** Boolean indicating success/failure

**Example:**
```json
{
  "notebook_name": "my-notebook",
  "page": "obsolete-note"
}
```

## Architecture

### Models
- `NotebookPage`: Represents a single page with text and timestamps
  - `Notebook`: Contains a collection of pages with metadata

### Services
- `INotebookStorageService`: Interface for storage operations
- `FileNotebookStorageService`: File-based storage implementation with semaphore-based concurrency control
- `INotebookService`: Business logic interface
- `NotebookService`: Main business logic implementation

### Tools
- `NotebookTools`: Unified class containing all MCP tools with attribute-based registration

## File Storage

Notebooks are stored in JSON format in the `notebooks` directory (configurable). Each notebook gets its own file:
- `notebooks/my-notebook.json`
- `notebooks/project-notes.json`

The JSON structure includes:
```json
{
  "Name": "my-notebook",
  "Pages": {
    "page1": {
      "Text": "text1",
      "CreatedAt": "2024-01-15T10:30:00Z",
      "ModifiedAt": "2024-01-15T10:30:00Z"
    }
  },
  "CreatedAt": "2024-01-15T10:30:00Z",
  "ModifiedAt": "2024-01-15T10:30:00Z"
}
```
Page names appear as keys under `Pages` and are not repeated inside each entry.

## Building and Running

### Prerequisites
- .NET 9.0 SDK or later

### Build
```cmd
dotnet build
```

### Run Tests
```cmd
dotnet test
```

### Run the Server
```cmd
cd src/NotebookMcpServer
dotnet run
```

The server will start and listen for MCP connections on stdin/stdout.

## Configuration

The server can be configured by modifying the DI container registration in `Program.cs`. Default settings:
- Storage directory: `./notebooks`
- Logging: Console output

## Configuration

The server can be configured in several ways:

### Environment Variables
- `NOTEBOOK_STORAGE_DIRECTORY`: Directory for storing notebook files (default: `./notebooks`)

### Configuration File (`appsettings.json`)
```json
{
  "Storage": {
    "Directory": "./notebooks"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "NotebookMcpServer": "Debug"
    }
  }
}
```

### Docker (Windows)
```cmd
docker run -d -e NOTEBOOK_STORAGE_DIRECTORY=C:\data\notebooks -v C:\host\notebooks:C:\data\notebooks ghcr.io/dimonsmart/notebook-mcp-server:latest
```

## Release and Deployment

### Pre-built Binaries
Download the latest release from [GitHub Releases](https://github.com/DimonSmart/NotebookMcpServer/releases):

- **Windows x64**: `NotebookMcpServer-win-x64.zip`

### Manual Build and Publish

#### Build for Development
```cmd
dotnet build --configuration Release
```

#### Publish Self-Contained Executable (Windows x64)
```cmd
dotnet publish src/NotebookMcpServer/NotebookMcpServer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64
```

#### Docker Build (Windows)
```cmd
# Build locally
docker build -t notebook-mcp-server .

# Run with volume mapping
docker run -it --rm -v %cd%/notebooks:/app/notebooks notebook-mcp-server
```

### CI/CD Pipeline
The project includes a GitHub Actions workflow that automatically:
- Builds and tests on every push and PR
- Creates release artifacts for all platforms on tags
- Publishes Docker images to GitHub Container Registry
- Creates GitHub releases with downloadable binaries

To create a new release:
1. Tag your commit: `git tag v1.0.0`
2. Push the tag: `git push origin v1.0.0`
3. GitHub Actions will automatically create the release

### Running Published Binaries

#### Windows
```cmd
# Extract NotebookMcpServer-win-x64.zip
NotebookMcpServer.exe
```

#### Configuration with Published Binaries
Set environment variables before running:
```cmd
set NOTEBOOK_STORAGE_DIRECTORY=C:\path\to\notebooks
NotebookMcpServer.exe
```

Or create `appsettings.json` in the same directory as the executable.

## Dependencies

- **MCPSharp** (v1.0.11): Modern Model Context Protocol implementation with attribute-based API
- **Microsoft.Extensions.DependencyInjection** (v9.0.8): Dependency injection
- **Microsoft.Extensions.Logging** (v9.0.8): Logging infrastructure
- **Microsoft.Extensions.Hosting** (v9.0.8): Hosting infrastructure
- **Microsoft.Extensions.Configuration** (v9.0.8): Configuration infrastructure

## Testing

The project includes comprehensive unit tests using xUnit:
- Service layer testing with in-memory storage
- Full coverage of business logic
- Edge case handling
- Error condition validation

Run tests with:
```cmd
dotnet test --verbosity normal
```

## Thread Safety

The server uses semaphores to ensure thread-safe file operations:
- One semaphore per notebook file
- Atomic write operations using temporary files
- Proper cleanup and resource disposal

## Error Handling

- Validates input parameters (null/empty checks)
- Handles file system errors gracefully
- Proper exception types for different error conditions
- Comprehensive logging for debugging

## Usage with MCP Clients

This server is designed to work with MCP-compatible clients. The client connects via stdio and can invoke the available tools to manage notebooks.

Example workflow:
1. Use `get_notebook_pages` to check existing pages
2. Use `upsert_page` to add or update pages
3. Use `remove_page` to remove obsolete pages

## Contributing

1. Ensure all tests pass: `dotnet test`
2. Follow the existing code style and architecture
3. Add tests for new functionality
4. Update documentation as needed

## License

[Add your license information here]
