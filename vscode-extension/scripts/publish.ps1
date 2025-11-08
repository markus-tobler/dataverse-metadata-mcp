# Publish Extension to VS Code Marketplace
# This script publishes the extension to the Visual Studio Code Marketplace

param(
    [Parameter(Mandatory = $false)]
    [string]$Version = "patch",
    
    [switch]$SkipTests = $false,
    
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"

Write-Host "Publishing Dataverse Metadata MCP Extension..." -ForegroundColor Cyan

# Get paths
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$extensionRoot = Split-Path -Parent $scriptPath

Push-Location $extensionRoot

try {
    # Run tests
    if (-not $SkipTests) {
        Write-Host "`nRunning tests..." -ForegroundColor Cyan
        npm test
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Tests failed. Aborting publish."
            exit 1
        }
        Write-Host "Tests passed" -ForegroundColor Green
    }
    
    # Build the extension
    if (-not $SkipBuild) {
        Write-Host "`nBuilding extension..." -ForegroundColor Cyan
        npm run package
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed. Aborting publish."
            exit 1
        }
        Write-Host "Build successful" -ForegroundColor Green
    }
    
    # Check if logged in to vsce
    Write-Host "`nChecking vsce login status..." -ForegroundColor Cyan
    $null = npx vsce ls-publishers 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Not logged in to vsce. Please login first:"
        Write-Host "  npx vsce login markus-tobler" -ForegroundColor Yellow
        exit 1
    }
    
    # Publish
    Write-Host "`nPublishing version: $Version" -ForegroundColor Cyan
    Write-Host "This will publish to the VS Code Marketplace. Continue? (Y/N)" -ForegroundColor Yellow
    $confirmation = Read-Host
    
    if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
        Write-Host "Publish cancelled" -ForegroundColor Yellow
        exit 0
    }
    
    npx vsce publish $Version --no-dependencies
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Publish failed"
        exit 1
    }
    
    Write-Host "`nExtension published successfully!" -ForegroundColor Green
    Write-Host "It may take a few minutes to appear in the marketplace." -ForegroundColor Gray
    
}
finally {
    Pop-Location
}
