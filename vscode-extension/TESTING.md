# Testing Guide for VS Code Extension

This guide helps you test the Dataverse Metadata MCP extension before publishing.

## Prerequisites

- VS Code 1.96.0 or later
- GitHub Copilot extension installed
- Access to a Dataverse environment
- The .vsix file: `dataverse-metadata-mcp-0.1.0.vsix`

## Test Scenarios

### Test 1: Extension Installation

**Steps:**

1. Locate the .vsix file in the extension directory
2. Install the extension:
   ```bash
   code --install-extension dataverse-metadata-mcp-0.1.0.vsix
   ```
3. Restart VS Code
4. Verify extension is listed: Extensions → Search for "Dataverse"

**Expected Result:**

- Extension appears in the extensions list
- Shows as "Dataverse Metadata MCP Server"
- Version 0.1.0

### Test 2: Extension Activation

**Steps:**

1. Open VS Code Output panel (View → Output)
2. Select "Extension Host" from dropdown
3. Look for activation message

**Expected Result:**

- Message: "Dataverse Metadata MCP extension is now active"
- No errors in output

### Test 3: Configuration Command

**Steps:**

1. Open Command Palette (Ctrl+Shift+P)
2. Type "Dataverse"
3. Select "Dataverse MCP: Configure Dataverse Connection"
4. Enter a connection string:
   ```
   AuthType=OAuth;Url=https://yourorg.crm.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/
   ```

**Expected Result:**

- Input box validates the connection string
- Rejects empty or invalid strings
- Success message after saving
- Setting saved to VS Code settings

### Test 4: Settings UI

**Steps:**

1. Open Settings (Ctrl+,)
2. Search for "Dataverse Metadata MCP"
3. Verify all settings appear

**Expected Result:**

- `enabled`: Boolean toggle (default: true)
- `connectionString`: String input (shows configured value)
- `serverPath`: String input (default: empty)

### Test 6: Server Installation and Detection

**Steps:**

1. Open Settings (Ctrl+,)
2. Search for "Dataverse Metadata MCP"
3. Verify all settings appear

**Expected Result:**

- `enabled`: Boolean toggle (default: true)
- `connectionString`: String input (shows configured value)
- `serverPath`: String input (default: empty)

### Test 5: MCP Server Registration with Copilot

**Steps:**

1. Ensure connection string is configured
2. Ensure extension is activated
3. Open any file in workspace
4. Open GitHub Copilot chat
5. Ask: "List all tables in my Dataverse environment"

**Expected Result:**

- Copilot recognizes the request
- MCP server starts (check processes for `dataverse-metadata-mcp-server`)
- Copilot returns list of Dataverse tables
- No error messages

### Test 6: Authentication Flow

**Steps:**

1. Configure a connection string with OAuth
2. Trigger an MCP tool via Copilot
3. Watch for authentication prompts

**Expected Result:**

- OAuth browser window opens (if needed)
- User authenticates
- Server connects successfully
- Tool returns data

### Test 9: Error Handling - No Connection String

**Steps:**

1. Clear the connection string in settings
2. Open Copilot chat
3. Ask: "Show me Dataverse tables"

**Expected Result:**

- Extension prompts for connection string
- Options: "Configure" or "Cancel"
- If Configure selected, input box appears
- If Cancel selected, operation aborts gracefully

### Test 10: Error Handling - Invalid Connection String

**Steps:**

1. Set an invalid connection string in settings
2. Try to use an MCP tool via Copilot

**Expected Result:**

- Server starts but fails to connect
- Error message appears
- User can reconfigure connection string

### Test 11: Multiple Commands

**Steps:**

1. Configure connection
2. Use multiple Copilot commands in sequence:
   - "List all Dataverse tables"
   - "Show columns for the Account table"
   - "What business process flows exist?"

**Expected Result:**

- All commands execute successfully
- Server remains running between calls
- No memory leaks or performance issues

### Test 12: Extension Uninstall

**Steps:**

1. Uninstall the extension:
   ```bash
   code --uninstall-extension markus-tobler.dataverse-metadata-mcp
   ```
2. Restart VS Code
3. Verify MCP tools no longer available

**Expected Result:**

- Extension removed cleanly
- No leftover processes
- Settings remain (can be manually deleted)

## Platform-Specific Tests

### Windows

- Test with PowerShell and CMD
- Verify .exe file detection
- Test OAuth authentication

### Linux

- Test executable permissions
- Verify path detection
- Test authentication flow

### macOS

- Test on Intel and ARM (if possible)
- Verify executable permissions
- Test keychain integration (if used)

## Performance Tests

### Test 1: Extension Activation Time

**Steps:**

1. Close VS Code
2. Open with `code --enable-proposed-api`
3. Check activation time in Output panel

**Expected Result:**

- Activation < 1 second
- No blocking operations

### Test 2: Server Startup Time

**Steps:**

1. Trigger first MCP tool call
2. Measure time to first response

**Expected Result:**

- Server starts < 3 seconds
- First response < 5 seconds

### Test 3: Memory Usage

**Steps:**

1. Check VS Code process memory
2. Use extension for 10+ minutes
3. Check memory again

**Expected Result:**

- No significant memory growth
- Server process stable memory usage

## Edge Cases

### Test 1: Concurrent Requests

- Send multiple Copilot requests quickly
- Verify server handles all requests

### Test 2: Server Crash Recovery

- Kill the server process manually
- Try to use MCP tool again
- Verify automatic restart

### Test 3: Network Interruption

- Start using MCP tools
- Disconnect network
- Reconnect
- Verify recovery

### Test 4: Workspace Changes

- Change VS Code workspace
- Verify settings persist or reset appropriately

## Checklist Before Publishing

- [ ] All tests pass on Windows
- [ ] All tests pass on Linux (if available)
- [ ] All tests pass on macOS (if available)
- [ ] No console errors in Extension Host output
- [ ] Server starts successfully every time
- [ ] Authentication works correctly
- [ ] All Copilot commands execute properly
- [ ] Configuration UI works correctly
- [ ] Error messages are helpful
- [ ] Extension uninstalls cleanly
- [ ] Performance is acceptable
- [ ] Memory usage is stable
- [ ] Documentation is accurate
- [ ] README has clear setup instructions
- [ ] CHANGELOG is up to date

## Reporting Issues

If you find issues during testing:

1. **Check Extension Host Output**: View → Output → "Extension Host"
2. **Check VS Code Console**: Help → Toggle Developer Tools → Console
3. **Check Server Logs**: Look for dataverse-metadata-mcp-server process output
4. **Document**:
   - Steps to reproduce
   - Expected vs actual behavior
   - VS Code version
   - Extension version
   - Operating system
   - Error messages/logs

## Automated Testing (Future)

Consider adding:

- Unit tests for extension logic
- Integration tests for MCP provider
- E2E tests with Copilot (if possible)
- CI/CD pipeline for testing

## Test Results Template

```
## Test Report

**Date**: YYYY-MM-DD
**Tester**: [Name]
**Environment**:
- OS: [Windows/Linux/macOS]
- VS Code: [version]
- Extension: [version]

**Results**:
- [ ] Test 1: Extension Installation
- [ ] Test 2: Extension Activation
- [ ] Test 3: Server Detection
- [ ] Test 4: Configuration Command
- [ ] Test 5: Settings UI
- [ ] Test 6: Server Installation
- [ ] Test 7: MCP Registration
- [ ] Test 8: Authentication Flow
- [ ] Test 9: Error Handling (No Config)
- [ ] Test 10: Error Handling (Invalid Config)
- [ ] Test 11: Multiple Commands
- [ ] Test 12: Extension Uninstall

**Issues Found**: [List any issues]

**Notes**: [Any additional observations]
```
