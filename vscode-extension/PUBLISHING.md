# Publishing Guide

This guide covers how to publish the Dataverse Metadata MCP extension to the Visual Studio Code Marketplace.

## 🚀 Recommended: GitHub Actions (Automated)

For automated, consistent, and auditable publishing, use **GitHub Actions**.

👉 **See [GITHUB-ACTIONS-PUBLISHING.md](./GITHUB-ACTIONS-PUBLISHING.md)** for the complete GitHub Actions setup guide.

### Quick Start with GitHub Actions

1. Add `VSCE_PAT` secret to your GitHub repository
2. Update version in `package.json`
3. Create a git tag: `git tag v0.2.0 && git push origin v0.2.0`
4. GitHub Actions automatically publishes! ✨

---

## 📝 Alternative: Manual Publishing (PowerShell/CLI)

If you prefer manual control or need to publish locally, follow the instructions below.

## Prerequisites

1. **Microsoft Account**: You need a Microsoft account to create a publisher
2. **Azure DevOps Organization**: Required for the marketplace
3. **Personal Access Token (PAT)**: For publishing via vsce

## One-Time Setup

### 1. Create a Publisher

1. Go to the [Visual Studio Marketplace Publisher Management](https://marketplace.visualstudio.com/manage)
2. Sign in with your Microsoft account
3. Click "Create publisher"
4. Fill in:
   - **Publisher ID**: `markus-tobler` (must match package.json)
   - **Publisher name**: Your display name
   - **Email**: Your contact email

### 2. Create a Personal Access Token

1. Go to [Azure DevOps](https://dev.azure.com/)
2. Click on User Settings (top right) → Personal Access Tokens
3. Click "New Token"
4. Configure:
   - **Name**: "VS Code Extension Publishing"
   - **Organization**: All accessible organizations
   - **Expiration**: Custom (set as needed)
   - **Scopes**: Select "Marketplace" → "Manage"
5. Click "Create" and **copy the token** (you won't see it again!)

### 3. Login with vsce

```bash
cd vscode-extension
npx vsce login markus-tobler
# Paste your PAT when prompted
```

## Building and Testing

### 1. Update Version

Update the version in `package.json`:

```json
{
  "version": "0.1.0"
}
```

### 2. Update CHANGELOG

Document changes in `CHANGELOG.md`

### 3. Build the Extension

```bash
npm install
npm run package
```

### 4. Test Locally

```bash
# Package the extension
npx vsce package

# Install locally for testing
code --install-extension dataverse-metadata-mcp-0.1.0.vsix

# Test thoroughly before publishing
```

### 5. Create Icon (if not done)

The extension needs a 128x128 PNG icon named `icon.png` in the root directory.

## Publishing

### First-Time Publish

```bash
# Make sure you're logged in
npx vsce login markus-tobler

# Publish the extension
npx vsce publish
```

### Subsequent Updates

```bash
# Option 1: Auto-increment version
npx vsce publish patch  # 0.1.0 -> 0.1.1
npx vsce publish minor  # 0.1.0 -> 0.2.0
npx vsce publish major  # 0.1.0 -> 1.0.0

# Option 2: Specify version
npx vsce publish 0.2.0
```

## Pre-Publish Checklist

- [ ] Version updated in package.json
- [ ] CHANGELOG.md updated with changes
- [ ] README.md is complete and accurate
- [ ] Icon file exists (icon.png, 128x128)
- [ ] Extension tested locally
- [ ] All dependencies are correctly listed
- [ ] License file exists
- [ ] Repository URL is correct
- [ ] Publisher ID matches in package.json
- [ ] No sensitive data in code or settings

## Verifying Publication

After publishing:

1. Go to the [Marketplace](https://marketplace.visualstudio.com/)
2. Search for "Dataverse Metadata MCP"
3. Verify all information is correct
4. Test installation from marketplace:
   ```bash
   code --install-extension markus-tobler.dataverse-metadata-mcp
   ```

## Updating Extension Metadata

To update the extension listing without publishing a new version:

1. Go to [Publisher Management](https://marketplace.visualstudio.com/manage/publishers/markus-tobler)
2. Click on your extension
3. Update description, screenshots, categories, etc.
4. Click "Update"

## Bundling the MCP Server

If you want to bundle the compiled MCP server with the extension:

1. Build the server:

   ```bash
   cd ../DataverseMetadataMcp.Server
   dotnet publish -c Release -o ../vscode-extension/server
   ```

2. Update `.vscodeignore` to include the server folder:

   ```
   !server/**
   ```

3. Test that the bundled server works:
   ```bash
   cd ../vscode-extension
   npm run package
   code --install-extension dataverse-metadata-mcp-0.1.0.vsix
   ```

## Unpublishing

If you need to unpublish an extension:

```bash
npx vsce unpublish markus-tobler.dataverse-metadata-mcp
```

⚠️ **Warning**: Unpublishing removes the extension for all users!

## CI/CD Automation

For automated publishing via GitHub Actions:

1. Add your PAT as a GitHub secret named `VSCE_PAT`
2. Create `.github/workflows/publish.yml`:

```yaml
name: Publish Extension

on:
  release:
    types: [created]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: "20"
      - run: npm ci
        working-directory: ./vscode-extension
      - run: npm run package
        working-directory: ./vscode-extension
      - run: npx vsce publish -p ${{ secrets.VSCE_PAT }}
        working-directory: ./vscode-extension
```

## Troubleshooting

### "Publisher not found"

Make sure:

- You've created a publisher on the marketplace
- The publisher ID in package.json matches exactly
- You're logged in with `vsce login`

### "Extension validation failed"

Common issues:

- Missing or invalid icon.png
- Invalid version format
- Missing required fields in package.json
- Files too large (check .vscodeignore)

### "Cannot publish extension"

- Verify your PAT is still valid
- Check that the PAT has "Marketplace (Manage)" permissions
- Try logging out and back in with vsce

## Resources

- [Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Extension Manifest](https://code.visualstudio.com/api/references/extension-manifest)
- [vsce CLI](https://github.com/microsoft/vscode-vsce)
- [Marketplace Publisher Management](https://marketplace.visualstudio.com/manage)
