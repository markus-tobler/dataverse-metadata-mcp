# Automated Release Process

This repository uses an automated release process for the VS Code extension. When a pull request is merged to `main` with a version bump, a release is automatically created and published to the VS Code Marketplace.

## 🔄 How It Works

### 1. **Pull Request Phase**

When you create a PR to `main` that modifies `vscode-extension/package.json`:

- ✅ **Version Validation** workflow runs automatically
- ✅ Checks that version in `package.json` is bumped (greater than current `main`)
- ✅ Warns if `CHANGELOG.md` is not updated
- ❌ PR cannot merge if version is not bumped

### 2. **Merge to Main**

When the PR is merged to `main`:

- ✅ **Auto Release** workflow triggers
- ✅ Reads version from `package.json`
- ✅ Creates a git tag (e.g., `v0.2.0`)
- ✅ Pushes the tag to GitHub

### 3. **Automatic Publishing**

When the tag is pushed:

- ✅ **Publish Extension** workflow triggers
- ✅ Builds MCP server for Windows, Linux, and macOS
- ✅ Packages the VS Code extension
- ✅ Publishes to VS Code Marketplace
- ✅ Publishes to Open VSX Registry (optional)
- ✅ Creates GitHub Release with VSIX file

## 📝 How to Release a New Version

### Step 1: Create a Branch

```bash
git checkout -b release/v0.2.0
```

### Step 2: Update Version

Edit `vscode-extension/package.json`:

```json
{
  "version": "0.2.0"
}
```

### Step 3: Update Changelog

Edit `vscode-extension/CHANGELOG.md`:

```markdown
## [0.2.0] - 2025-11-08

### Added

- New feature X
- Enhancement Y

### Fixed

- Bug Z
```

### Step 4: Commit and Push

```bash
git add vscode-extension/package.json vscode-extension/CHANGELOG.md
git commit -m "Release v0.2.0"
git push origin release/v0.2.0
```

### Step 5: Create Pull Request

1. Go to GitHub and create a PR from your branch to `main`
2. The **Version Validation** workflow will run
3. Verify it passes ✅
4. Get PR reviewed and approved
5. Merge the PR

### Step 6: Watch Automation

After merge:

1. **Auto Release** workflow creates tag `v0.2.0`
2. **Publish Extension** workflow builds and publishes
3. Check **Actions** tab to monitor progress
4. Extension appears on [VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=markus-tobler.dataverse-metadata-mcp)

## 🎯 Version Numbering

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.0.0): Breaking changes
- **MINOR** (0.2.0): New features, backwards compatible
- **PATCH** (0.1.1): Bug fixes

## ✅ Prerequisites (One-Time Setup)

### 1. Create VS Code Marketplace PAT

1. Go to [Azure DevOps](https://dev.azure.com/)
2. User Settings → Personal Access Tokens → New Token
3. Configure:
   - **Name**: "GitHub Actions Publishing"
   - **Organization**: All accessible organizations
   - **Scopes**: **Marketplace → Manage**
4. Copy the token

### 2. Add GitHub Secret

1. Go to your repository **Settings → Secrets → Actions**
2. Click **New repository secret**
3. Add: `VSCE_PAT` = your marketplace token

### 3. Create Publisher (if not exists)

1. Go to [Publisher Management](https://marketplace.visualstudio.com/manage)
2. Create publisher with ID: `markus-tobler` (must match `package.json`)

## 🚨 Troubleshooting

### PR Check Fails: "Version must be bumped"

**Cause**: Version in your PR matches version on `main`

**Fix**: Increase version in `package.json`

### Auto Release Workflow Doesn't Run

**Cause**: No changes to `package.json` or workflow disabled

**Fix**: Verify `package.json` was actually changed in the merge

### Publish Fails: "Tag version doesn't match package.json"

**Cause**: Manual tag created that doesn't match `package.json`

**Fix**: Delete the tag and let auto-release create it:

```bash
git tag -d v0.2.0
git push origin :refs/tags/v0.2.0
```

### Publish Fails: "Authentication Failed"

**Cause**: Invalid or expired `VSCE_PAT`

**Fix**: Create new PAT and update GitHub secret

## 🔧 Manual Release (Emergency)

If automation fails, you can manually trigger a release:

1. Go to **Actions** tab
2. Select **"Publish Extension to Marketplace"**
3. Click **"Run workflow"**
4. Enter version (e.g., `0.2.0`)
5. Click **"Run workflow"**

## 📊 Workflow Files

- `.github/workflows/validate-version.yml` - PR validation
- `.github/workflows/auto-release.yml` - Auto tag creation
- `.github/workflows/publish-extension.yml` - Marketplace publishing
- `.github/workflows/extension-ci.yml` - CI testing

## 🎉 Benefits

✅ **Consistent Process**: Same steps every time  
✅ **No Manual Tags**: Version in `package.json` is source of truth  
✅ **PR Validation**: Prevents version conflicts  
✅ **Audit Trail**: All releases tracked in GitHub  
✅ **Community Friendly**: External contributors can follow same process  
✅ **Fast**: Typically completes in 5-10 minutes

## 📖 For Contributors

When contributing:

1. Don't bump version unless it's a release PR
2. Maintainers will handle version bumps and releases
3. Focus on your feature/fix in isolated PRs
4. Version bumps happen in dedicated release PRs
