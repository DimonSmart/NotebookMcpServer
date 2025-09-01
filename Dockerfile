FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Expose any ports if needed for HTTP transport
# EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/NotebookMcpServer/NotebookMcpServer.csproj", "src/NotebookMcpServer/"]
COPY ["tests/NotebookMcpServer.Tests/NotebookMcpServer.Tests.csproj", "tests/NotebookMcpServer.Tests/"]
COPY ["Directory.Build.props", "."]
RUN dotnet restore "src/NotebookMcpServer/NotebookMcpServer.csproj"

COPY . .
WORKDIR "/src/src/NotebookMcpServer"
RUN dotnet build "NotebookMcpServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotebookMcpServer.csproj" -c Release -o /app/publish /p:PublishTrimmed=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create notebooks directory and set permissions
RUN mkdir -p /app/notebooks && chmod 755 /app/notebooks

# Environment variables for configuration
ENV NOTEBOOK_STORAGE_DIRECTORY=/app/notebooks
ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "NotebookMcpServer.dll"]
