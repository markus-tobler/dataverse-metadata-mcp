# Install Extension Locally
# This script installs the packaged extension locally for testing

param(
    [switch]$Uninstall = $false
)

$ErrorActionPreference = "Stop"

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$extensionRoot = Split-Path -Parent $scriptPath

Push-Location $extensionRoot

try {
    if ($Uninstall) {
        Write-Host "Uninstalling extension..." -ForegroundColor Cyan
        code --uninstall-extension markus-tobler.dataverse-metadata-mcp
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Extension uninstalled successfully" -ForegroundColor Green
        }
    }
    else {
        # Find the .vsix file
        $vsixFile = Get-ChildItem -Filter "*.vsix" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        
        if (-not $vsixFile) {
            Write-Error "No .vsix file found. Please build the extension first:"
            Write-Host "  npm run package" -ForegroundColor Yellow
            Write-Host "  npx vsce package" -ForegroundColor Yellow
            exit 1
        }
        
        Write-Host "Installing extension from: $($vsixFile.Name)" -ForegroundColor Cyan
        code --install-extension $vsixFile.FullName --force
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "`nExtension installed successfully!" -ForegroundColor Green
            Write-Host "Please reload VS Code to activate the extension." -ForegroundColor Yellow
        }
        else {
            Write-Error "Installation failed"
            exit 1
        }
    }
}
finally {
    Pop-Location
}
