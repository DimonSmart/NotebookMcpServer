# Публикация MCP-сервера

## Подготовка пакета

1. Добавьте в `NotebookMcpServer.csproj` метаданные NuGet:
   ```xml
   <PropertyGroup>
     <PackageId>NotebookMcpServer</PackageId>
     <Version>1.0.0</Version>
     <Authors>YOUR_NAME</Authors>
     <PackageDescription>Notebook MCP Server</PackageDescription>
   </PropertyGroup>
   ```
2. Соберите пакет:
   ```bash
   dotnet pack src/NotebookMcpServer -c Release
   ```

## Публикация на NuGet

1. Создайте API-ключ на [nuget.org](https://www.nuget.org/).
2. Выполните загрузку:
   ```bash
   dotnet nuget push src/NotebookMcpServer/bin/Release/NotebookMcpServer.1.0.0.nupkg \
     --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
   ```

## Дебаг и подключение

Для отладки в Visual Studio или другом клиенте можно запускать сервер напрямую:
```bash
dotnet run --project src/NotebookMcpServer
```
В конфигурации клиента укажите путь к исполняемому файлу, чтобы подключить MCP-сервер.
