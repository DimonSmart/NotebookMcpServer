# Notebook MCP Server

🚀 **Unlock persistent memory for your AI assistants!** Transform any LLM or Copilot-like tool into a thoughtful companion that remembers across conversations. This lightweight MCP server gives your AI clients the superpower of local, private notebooks — think of it as a digital brain that never forgets.

## Why Choose Notebook MCP Server?

* **🔒 Local & Private** — Everything stays on your machine as plain JSON files. No cloud, no accounts, no data mining.
* **🌐 Universal Compatibility** — Works seamlessly with any MCP-compatible client (LLMs, IDE assistants, AI agents).
* **⚡ Lightning Fast** — Optimized for instant reads and writes on local storage.
* **🛡️ Zero Telemetry** — Your data is yours, period.
* **🎯 Project Isolation** — Smart design prevents different projects from interfering with each other while enabling secure notebook sharing when needed.

## Core Features

* **Named Notebooks** — Organize information in logically separated notebooks
* **Page-Based Storage** — Each notebook contains multiple pages of text content
* **JSON Persistence** — One clean JSON file per notebook for easy backup and portability
* **Isolated Access** — No "list all notebooks" function ensures project privacy
* **Secure Sharing** — Share specific notebooks between projects by name when desired
* **Minimal API Surface** — Clean, focused toolset without unnecessary complexity

## Quick Start

1. **Download** the latest binary from [Releases](../../releases).
2. **Register** this server in your MCP-compatible client (via stdio).
   See your client's documentation for how to add an MCP server.
3. **Start using** the tools below from your AI client.

> **Default storage directory:** `./notebooks`  
> **Custom location:** Set environment variable `NOTEBOOK_STORAGE_DIRECTORY=/path/to/notebooks`

## Available Tools (MCP API)

### 📚 Notebook Management
* **`create_notebook`** — Create a new notebook or update its description
  - Parameters: `notebookName` (string), `description` (string)
  - Returns: Confirmation message

### 📄 Page Operations
* **`get_notebook_page_names`** — Get notebook description and list of page names (without content)
  - Parameters: `notebookName` (string)
  - Returns: `NotebookSummary` with description and page names
  - 🔒 *Note: This is the only way to discover pages — no global notebook listing for privacy*

* **`get_page_text`** — Read the full text content of a specific page
  - Parameters: `notebookName` (string), `page` (string)
  - Returns: Page text content (empty string if page doesn't exist)

* **`upsert_page`** — Create a new page or update existing page content
  - Parameters: `notebookName` (string), `page` (string), `text` (string)
  - Returns: Confirmation message
  - 💡 *Creates notebook automatically if it doesn't exist*

* **`remove_page`** — Delete a page from a notebook
  - Parameters: `notebookName` (string), `page` (string)
  - Returns: `true` if page was deleted, `false` if page didn't exist

### 🛡️ Privacy by Design
**Important:** There is intentionally **no** "list all notebooks" function. This ensures that different projects cannot interfere with each other's data. However, if you know a notebook name, you can share it between projects securely.

## Data Storage Format

Each notebook is stored as a single JSON file in your storage directory. The structure is simple and human-readable:

```json
{
  "description": "My project notes",
  "pages": {
    "page-name-1": "Content of first page...",
    "page-name-2": "Content of second page...",
    "meeting-notes": "Notes from today's meeting..."
  }
}
```

Page names serve as keys, and page text is stored as values. This format makes it easy to backup, version control, or manually edit your notebooks if needed.

## Compatibility & Integration

Works with any **MCP client** that supports stdio tool execution, including:
- Claude Desktop
- VS Code Copilot extensions
- Custom LLM agents
- IDE assistants
- AI workflow tools

Perfect as a lightweight, local memory system alongside Copilot-style assistants.

## Contributing

Issues and pull requests are welcome! Feel free to:
- Report bugs or suggest features
- Improve documentation
- Submit code improvements
- Share usage examples

## License — 0BSD

Copyright (c) 2025 DimonSmart

Permission to use, copy, modify, and/or distribute this software for any purpose with or without fee is hereby granted.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
