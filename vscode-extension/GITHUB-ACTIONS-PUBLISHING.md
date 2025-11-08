# GitHub Actions Publishing Guide

This guide explains how to publish the VS Code extension using GitHub Actions instead of manual PowerShell scripts.

## Overview

The repository includes two GitHub Actions workflows:

1. **`publish-extension.yml`**: Publishes the extension to VS Code Marketplace
2. **`extension-ci.yml`**: Runs tests and builds on every push/PR

## Advantages of GitHub Actions

✅ **Automated Publishing**: Publish automatically on version tags  
✅ **Consistent Builds**: Same environment every time  
✅ **Multi-Platform Testing**: Test on Windows, Linux, and macOS  
✅ **No Local Setup**: No need to install build tools locally  
✅ **Audit Trail**: Complete history of all releases  
✅ **Secret Management**: Secure storage of API tokens  
✅ **GitHub Releases**: Automatic creation with VSIX attachments

## Setup Instructions

### 1. Create Personal Access Token (PAT)

#### VS Code Marketplace PAT

1. Go to [Azure DevOps](https://dev.azure.com/)
2. Click User Settings → Personal Access Tokens
3. Click "New Token"
4. Configure:
   - **Name**: "GitHub Actions Publishing"
   - **Organization**: All accessible organizations
   - **Expiration**: 1 year (or longer)
   - **Scopes**: Select **"Marketplace" → "Manage"**
5. Click "Create" and **copy the token**

#### Open VSX PAT (Optional)

Open VSX is an alternative marketplace for VS Code extensions.

1. Go to [Open VSX](https://open-vsx.org/)
2. Sign in with GitHub
3. Go to [Access Token Settings](https://open-vsx.org/user-settings/tokens)
4. Generate a new access token
5. Copy the token

### 2. Add Secrets to GitHub Repository

1. Go to your repository on GitHub
2. Navigate to **Settings → Secrets and variables → Actions**
3. Click **"New repository secret"**
4. Add the following secrets:

   | Secret Name | Value                        | Description                                    |
   | ----------- | ---------------------------- | ---------------------------------------------- |
   | `VSCE_PAT`  | Your VS Code Marketplace PAT | Required for publishing to VS Code Marketplace |
   | `OVSX_PAT`  | Your Open VSX PAT            | Optional: For publishing to Open VSX Registry  |

### 3. Create a Publisher (One-Time)

1. Go to [Visual Studio Marketplace Publisher Management](https://marketplace.visualstudio.com/manage)
2. Sign in with your Microsoft account
3. Click "Create publisher"
4. Fill in:
   - **Publisher ID**: `markus-tobler` (must match package.json)
   - **Publisher name**: Your display name
   - **Email**: Your contact email

## Publishing Workflows

### Option 1: Publish on Git Tag (Recommended)

This is the most common approach for version releases.

#### Steps:

1. **Update version in `package.json`**:

   ```json
   {
     "version": "0.2.0"
   }
   ```

2. **Update `CHANGELOG.md`**:

   ```markdown
   ## [0.2.0] - 2025-11-08

   ### Added

   - New feature X
   ```

3. **Commit and push changes**:

   ```bash
   git add vscode-extension/package.json vscode-extension/CHANGELOG.md
   git commit -m "Bump version to 0.2.0"
   git push
   ```

4. **Create and push a version tag**:

   ```bash
   git tag v0.2.0
   git push origin v0.2.0
   ```

5. **GitHub Actions automatically**:
   - Builds the MCP server for all platforms
   - Compiles the extension
   - Runs tests
   - Publishes to VS Code Marketplace
   - Creates a GitHub Release with the VSIX file

#### Monitor Progress:

- Go to **Actions** tab in GitHub
- Watch the "Publish Extension to Marketplace" workflow
- Check for any errors

### Option 2: Manual Trigger from GitHub UI

You can trigger a publish manually without creating a tag.

#### Steps:

1. Go to **Actions** tab in GitHub
2. Select **"Publish Extension to Marketplace"** workflow
3. Click **"Run workflow"**
4. Select branch and enter version (e.g., `0.2.0`, `patch`, `minor`, `major`)
5. Click **"Run workflow"**

This is useful for:

- Testing the publishing process
- Publishing hotfixes
- Re-publishing after a failed attempt

## Workflow Details

### Build Process

The `publish-extension.yml` workflow:

1. **Job 1: Build Servers** (runs on Windows)

   - Builds MCP server for Windows (win-x64)
   - Builds MCP server for Linux (linux-x64)
   - Builds MCP server for macOS (osx-x64)
   - Uploads binaries as artifacts

2. **Job 2: Publish** (runs on Ubuntu)
   - Downloads server binaries
   - Installs npm dependencies
   - Runs tests (optional)
   - Builds extension
   - Packages VSIX
   - Publishes to VS Code Marketplace
   - Publishes to Open VSX (optional)
   - Creates GitHub Release (for tags)
   - Uploads VSIX as artifact

### CI Testing

The `extension-ci.yml` workflow runs on every push/PR:

1. **Build and Test** (matrix: Windows, Linux, macOS)

   - Builds MCP server for the platform
   - Installs dependencies
   - Runs linter
   - Type checks TypeScript
   - Runs tests
   - Builds extension
   - Packages VSIX

2. **Validate Package**
   - Downloads VSIX
   - Checks package size
   - Lists contents

## Version Management

### Semantic Versioning

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.0.0): Breaking changes
- **MINOR** (0.1.0): New features, backwards compatible
- **PATCH** (0.0.1): Bug fixes

### Version Bumping

You can specify version in multiple ways:

```bash
# Specific version
git tag v0.2.0

# Or use workflow input:
# - "patch": 0.1.0 → 0.1.1
# - "minor": 0.1.0 → 0.2.0
# - "major": 0.1.0 → 1.0.0
```

## Troubleshooting

### Workflow Fails with "Authentication Failed"

**Cause**: Invalid or expired PAT

**Fix**:

1. Generate a new PAT in Azure DevOps
2. Update the `VSCE_PAT` secret in GitHub
3. Re-run the workflow

### Workflow Fails with "Publisher Not Found"

**Cause**: Publisher not created or ID mismatch

**Fix**:

1. Create publisher at https://marketplace.visualstudio.com/manage
2. Ensure publisher ID matches `package.json`
3. Wait a few minutes and retry

### Server Build Fails

**Cause**: Missing .NET SDK or build errors

**Fix**:

1. Check the workflow logs
2. Ensure .NET 9.0 is specified correctly
3. Test build locally: `dotnet build`

### Package Size Too Large

**Cause**: Large server binaries or unnecessary files

**Fix**:

1. Check `.vscodeignore` excludes unnecessary files
2. Use `--self-contained false` for smaller binaries
3. Review workflow logs for "Package size" warning

### Test Failures

**Cause**: Tests failing in CI environment

**Fix**:

1. Set `continue-on-error: true` temporarily (already set)
2. Add proper tests when ready
3. Remove `continue-on-error` to enforce passing tests

## Viewing Published Extension

After successful publish:

1. **VS Code Marketplace**: https://marketplace.visualstudio.com/items?itemName=markus-tobler.dataverse-metadata-mcp
2. **GitHub Releases**: https://github.com/markus-tobler/dataverse-metadata-mcp/releases
3. **Install**: `code --install-extension markus-tobler.dataverse-metadata-mcp`

## Rollback

If you need to unpublish or rollback:

### Unpublish from Marketplace

```bash
npx vsce unpublish markus-tobler.dataverse-metadata-mcp
```

⚠️ **Warning**: This removes the extension for all users!

### Publish Previous Version

```bash
# Checkout previous version
git checkout v0.1.0

# Manually publish
cd vscode-extension
npx vsce publish -p $VSCE_PAT
```

## Comparing with PowerShell Script

| Feature            | PowerShell Script | GitHub Actions   |
| ------------------ | ----------------- | ---------------- |
| **Automation**     | Manual execution  | Automatic on tag |
| **Environment**    | Local machine     | Cloud runners    |
| **Multi-platform** | Manual setup      | Built-in matrix  |
| **Secrets**        | Local env vars    | GitHub Secrets   |
| **Audit trail**    | None              | Full history     |
| **GitHub Release** | Manual            | Automatic        |
| **Server build**   | Manual script     | Integrated       |
| **Testing**        | Optional          | Enforced         |

## Best Practices

1. **Always test locally first** with `npm run compile` and `npm test`
2. **Use semantic versioning** for clear version history
3. **Update CHANGELOG.md** before tagging
4. **Test the VSIX** before publishing: `code --install-extension *.vsix`
5. **Review workflow logs** after each publish
6. **Keep PAT secure** and rotate periodically
7. **Use branch protection** to require CI before merge
8. **Tag from main branch** for production releases

## Migration from PowerShell

If you want to keep both options:

1. **Keep PowerShell scripts** for local testing
2. **Use GitHub Actions** for production releases
3. **Scripts remain in `scripts/`** folder
4. **Update documentation** to recommend GitHub Actions

The workflows are already set up and ready to use! 🚀

## Next Steps

1. ✅ Workflows are created in `.github/workflows/`
2. ⬜ Add `VSCE_PAT` secret to GitHub repository
3. ⬜ Create publisher on VS Code Marketplace (if not done)
4. ⬜ Test with a version tag: `git tag v0.1.1 && git push origin v0.1.1`
5. ⬜ Monitor the Actions tab for results
