# Release Checklist

## Prerequisites
- .NET 9.0 SDK or later installed
- Docker (optional, for container builds)
- Write access to the GitHub repository

## Local Release Build

### 1. Prepare Release
```bash
# Update version in csproj if needed
# Create and push git tag
git tag v1.0.0
git push origin v1.0.0
```

### 2. Build and Test
```bash
# Build project
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release --verbosity normal

# Verify all tests pass
```

### 3. Create Release Binaries
```bash
# Windows x64
dotnet publish src/NotebookMcpServer/NotebookMcpServer.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -o publish/win-x64

# Linux x64
dotnet publish src/NotebookMcpServer/NotebookMcpServer.csproj \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -o publish/linux-x64

# Linux ARM64
dotnet publish src/NotebookMcpServer/NotebookMcpServer.csproj \
  -c Release -r linux-arm64 --self-contained true \
  -p:PublishSingleFile=true -o publish/linux-arm64

# macOS x64
dotnet publish src/NotebookMcpServer/NotebookMcpServer.csproj \
  -c Release -r osx-x64 --self-contained true \
  -p:PublishSingleFile=true -o publish/osx-x64

# macOS ARM64
dotnet publish src/NotebookMcpServer/NotebookMcpServer.csproj \
  -c Release -r osx-arm64 --self-contained true \
  -p:PublishSingleFile=true -o publish/osx-arm64
```

### 4. Create Archives
```bash
# Create ZIP for Windows
cd publish/win-x64
zip -r ../../NotebookMcpServer-win-x64.zip .
cd ../..

# Create tar.gz for Linux/macOS
cd publish/linux-x64
tar -czf ../../NotebookMcpServer-linux-x64.tar.gz .
cd ../..

cd publish/linux-arm64
tar -czf ../../NotebookMcpServer-linux-arm64.tar.gz .
cd ../..

cd publish/osx-x64
tar -czf ../../NotebookMcpServer-osx-x64.tar.gz .
cd ../..

cd publish/osx-arm64
tar -czf ../../NotebookMcpServer-osx-arm64.tar.gz .
cd ../..
```

### 5. Test Release Binaries
```bash
# Test Windows (on Windows machine)
cd publish/win-x64
echo "" | .\NotebookMcpServer.exe

# Test Linux (on Linux machine)
cd publish/linux-x64
echo "" | ./NotebookMcpServer

# Test with configuration
export NOTEBOOK_STORAGE_DIRECTORY=./test-notebooks
echo "" | ./NotebookMcpServer
```

## Docker Release

### 1. Build Docker Image
```bash
docker build -t notebook-mcp-server:latest .
docker build -t notebook-mcp-server:v1.0.0 .
```

### 2. Test Docker Image
```bash
docker run --rm -v $(pwd)/test-notebooks:/app/notebooks notebook-mcp-server:latest
```

### 3. Push to Registry (if automated)
```bash
# GitHub Container Registry
docker tag notebook-mcp-server:latest ghcr.io/dimonsmart/notebook-mcp-server:latest
docker tag notebook-mcp-server:v1.0.0 ghcr.io/dimonsmart/notebook-mcp-server:v1.0.0
docker push ghcr.io/dimonsmart/notebook-mcp-server:latest
docker push ghcr.io/dimonsmart/notebook-mcp-server:v1.0.0
```

## Automated Release (Recommended)

### GitHub Actions
The project includes a complete CI/CD pipeline that automatically:

1. **On every push/PR**: Runs build and tests
2. **On tag push (v*)**: 
   - Creates release binaries for all platforms
   - Builds and pushes Docker images
   - Creates GitHub release with downloadable assets

### Triggering Automated Release
```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0

# GitHub Actions will automatically:
# - Build for all platforms
# - Run tests
# - Create release archives
# - Build Docker images
# - Create GitHub release
```

## Post-Release Checklist

- [ ] Verify release artifacts are downloadable
- [ ] Test downloaded binaries on target platforms
- [ ] Update documentation if needed
- [ ] Announce release (if public)
- [ ] Update any dependent projects

## Configuration for Released Binaries

Users can configure the released binaries using:

1. **Environment Variables:**
   ```bash
   export NOTEBOOK_STORAGE_DIRECTORY=/path/to/notebooks
   ./NotebookMcpServer
   ```

2. **appsettings.json** (place next to executable):
   ```json
   {
     "Storage": {
       "Directory": "/path/to/notebooks"
     }
   }
   ```

3. **Docker:**
   ```bash
   docker run -d \
     -e NOTEBOOK_STORAGE_DIRECTORY=/data/notebooks \
     -v /host/path:/data/notebooks \
     notebook-mcp-server:latest
   ```
