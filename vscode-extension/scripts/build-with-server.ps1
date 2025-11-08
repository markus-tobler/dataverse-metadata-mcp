# Build Extension with Bundled Server
# This script builds the MCP server and bundles it with the VS Code extension

param(
    [switch]$SkipServerBuild = $false
)

$ErrorActionPreference = "Stop"

Write-Host "Building Dataverse Metadata MCP Extension with bundled server..." -ForegroundColor Cyan

# Get paths
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$extensionRoot = Split-Path -Parent $scriptPath
$repoRoot = Split-Path -Parent $extensionRoot
$serverProject = Join-Path $repoRoot "DataverseMetadataMcp.Server\DataverseMetadataMcp.Server.csproj"
$serverOutput = Join-Path $extensionRoot "server"

# Clean old server files
if (Test-Path $serverOutput) {
    Write-Host "Cleaning old server files..." -ForegroundColor Yellow
    Remove-Item -Path $serverOutput -Recurse -Force
}

if (-not $SkipServerBuild) {
    # Build the .NET MCP server
    Write-Host "`nBuilding MCP server..." -ForegroundColor Cyan
    
    # Build for Windows
    Write-Host "  Building for win-x64..." -ForegroundColor Gray
    dotnet publish $serverProject `
        -c Release `
        -r win-x64 `
        --self-contained false `
        -o "$serverOutput\win-x64" `
        /p:PublishSingleFile=true `
        /p:DebugType=None `
        /p:DebugSymbols=false
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build MCP server for Windows"
        exit 1
    }
    
    # Build for Linux
    Write-Host "  Building for linux-x64..." -ForegroundColor Gray
    dotnet publish $serverProject `
        -c Release `
        -r linux-x64 `
        --self-contained false `
        -o "$serverOutput\linux-x64" `
        /p:PublishSingleFile=true `
        /p:DebugType=None `
        /p:DebugSymbols=false
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build MCP server for Linux"
        exit 1
    }
    
    # Build for macOS
    Write-Host "  Building for osx-x64..." -ForegroundColor Gray
    dotnet publish $serverProject `
        -c Release `
        -r osx-x64 `
        --self-contained false `
        -o "$serverOutput\osx-x64" `
        /p:PublishSingleFile=true `
        /p:DebugType=None `
        /p:DebugSymbols=false
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build MCP server for macOS"
        exit 1
    }
    
    Write-Host "MCP server built successfully" -ForegroundColor Green
}
else {
    Write-Host "Skipping server build (using existing server files)" -ForegroundColor Yellow
}

# Build the extension
Write-Host "`nBuilding VS Code extension..." -ForegroundColor Cyan
Push-Location $extensionRoot

try {
    # Install dependencies if needed
    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing npm dependencies..." -ForegroundColor Gray
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to install npm dependencies"
            exit 1
        }
    }
    
    # Compile the extension
    Write-Host "Compiling TypeScript..." -ForegroundColor Gray
    npm run package
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to compile extension"
        exit 1
    }
    
    Write-Host "Extension built successfully" -ForegroundColor Green
    
    # Package the extension
    Write-Host "`nPackaging extension..." -ForegroundColor Cyan
    npx vsce package --no-dependencies
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to package extension"
        exit 1
    }
    
    # Get the generated .vsix file
    $vsixFile = Get-ChildItem -Filter "*.vsix" | Select-Object -First 1
    
    if ($vsixFile) {
        Write-Host "`nExtension packaged successfully!" -ForegroundColor Green
        Write-Host "VSIX file: $($vsixFile.FullName)" -ForegroundColor Cyan
        Write-Host "`nTo install locally, run:" -ForegroundColor Yellow
        Write-Host "  code --install-extension $($vsixFile.Name)" -ForegroundColor Gray
    }
    
}
finally {
    Pop-Location
}

Write-Host "`nBuild complete!" -ForegroundColor Green
